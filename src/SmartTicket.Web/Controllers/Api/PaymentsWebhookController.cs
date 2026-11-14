using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Payments;
using SmartTicket.Application.Bookings;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Web.Hubs;

namespace SmartTicket.Web.Controllers.Api;

[ApiController, Route("api/payments")]
public sealed class PaymentsWebhookController(
    IPaymentService payments,
    IConfiguration configuration,
    IHubContext<TripSeatsHub> hub,
    IHostEnvironment env,
    SmartTicketDbContext db,
    ILogger<PaymentsWebhookController> logger) : ControllerBase
{
    [HttpGet("webhook"), HttpGet("sepay-webhook"), AllowAnonymous]
    public IActionResult WebhookHealth() =>
        Ok(new { status = "ready", endpoint = "SmartTicket SePay Webhook Receiver" });

    [HttpPost("webhook"), HttpPost("sepay-webhook"), AllowAnonymous, RequestSizeLimit(65536)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var secret = configuration["Payments:WebhookSecret"] ?? "";
        var sePayApiKey = configuration["Payments:SePayApiKey"] ?? "";
        var hasValidHmacSecret = Encoding.UTF8.GetByteCount(secret) >= 32;
        if (!env.IsDevelopment() && !hasValidHmacSecret && string.IsNullOrWhiteSpace(sePayApiKey))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Webhook authentication is not configured." });

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(ct);

        logger.LogInformation("Nhận webhook thanh toán tại {Path} (Bytes: {Length})", Request.Path, raw.Length);

        // 1. Kiểm tra xác thực (Hỗ trợ X-Signature HMAC-SHA256 và Authorization Apikey/Bearer của SePay)
        var signatureHeader = Request.Headers["X-Signature"].ToString();
        var authHeader = Request.Headers["Authorization"].ToString();

        var isAuthenticated = false;

        if (!string.IsNullOrWhiteSpace(signatureHeader) && hasValidHmacSecret)
        {
            if (signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)) signatureHeader = signatureHeader[7..];
            try
            {
                var provided = Convert.FromHexString(signatureHeader);
                var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(raw));
                isAuthenticated = CryptographicOperations.FixedTimeEquals(expected, provided);
            }
            catch { isAuthenticated = false; }
        }
        else if (!string.IsNullOrWhiteSpace(authHeader) && (!string.IsNullOrWhiteSpace(sePayApiKey) || hasValidHmacSecret))
        {
            var key = authHeader.Replace("Apikey ", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase)
                                .Trim();
            isAuthenticated = (!string.IsNullOrWhiteSpace(sePayApiKey) && string.Equals(key, sePayApiKey, StringComparison.Ordinal))
                           || (hasValidHmacSecret && string.Equals(key, secret, StringComparison.Ordinal));
        }
        else if (env.IsDevelopment() || (string.IsNullOrWhiteSpace(secret) && string.IsNullOrWhiteSpace(sePayApiKey)))
        {
            // Trong môi trường Development hoặc khi chưa đặt khóa, cho phép webhook không kèm header xác thực đi qua để test
            isAuthenticated = true;
        }

        if (!isAuthenticated)
        {
            logger.LogWarning("Từ chối webhook thanh toán do sai chữ ký hoặc API Key (HasSignature={HasSig}, HasAuth={HasAuth}).",
                !string.IsNullOrWhiteSpace(signatureHeader), !string.IsNullOrWhiteSpace(authHeader));
            return Unauthorized(new { error = "Chữ ký hoặc API Key webhook không hợp lệ." });
        }

        PaymentWebhookPayload? payload = null;
        var isSePayShape = false;

        // 2. Thử phân giải theo định dạng SePay webhook (content, description, code, transferAmount, transferType, referenceCode)
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                isSePayShape = root.TryGetProperty("transferType", out _)
                            || root.TryGetProperty("transferAmount", out _)
                            || root.TryGetProperty("gateway", out _)
                            || (root.TryGetProperty("accountNumber", out _) && root.TryGetProperty("content", out _));

                var combinedText = string.Join(" ",
                    ReadJsonString(root, "content"),
                    ReadJsonString(root, "description"),
                    ReadJsonString(root, "code"));

                var match = System.Text.RegularExpressions.Regex.Match(combinedText, @"STKT\s*[A-Za-z0-9]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var refCode = match.Value.Replace(" ", "").ToUpperInvariant();
                    var txId = ReadJsonId(root, "id")
                        ?? ReadJsonId(root, "referenceCode")
                        ?? Guid.NewGuid().ToString("N");

                    var amount = ReadJsonDecimal(root, "transferAmount") ?? ReadJsonDecimal(root, "amount") ?? 0m;
                    var transferType = ReadJsonString(root, "transferType");
                    if (string.IsNullOrWhiteSpace(transferType)) transferType = "in";
                    var status = string.Equals(transferType, "in", StringComparison.OrdinalIgnoreCase) ? "SUCCESS" : "FAILED";

                    payload = new PaymentWebhookPayload(refCode, txId, amount, status);
                }
            }
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "Dữ liệu webhook không đúng định dạng JSON." });
        }

        if (payload is null && isSePayShape)
        {
            logger.LogInformation("Nhận sự kiện SePay nhưng không có mã STKT... trong nội dung (giao dịch ngoài hệ thống hoặc test ping). Bỏ qua an toàn.");
            return Ok(new { success = true, processed = false, result = "ignored_no_reference" });
        }

        // 3. Nếu chưa parse được SePay, thử parse theo định dạng chuẩn SmartTicket (Reference, TransactionId, Amount, Status)
        if (payload is null)
        {
            try
            {
                payload = JsonSerializer.Deserialize<PaymentWebhookPayload>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Dữ liệu webhook không đúng định dạng JSON." });
            }
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Reference))
        {
            return BadRequest(new { error = "Không tìm thấy mã tham chiếu thanh toán (STKT...) trong nội dung webhook." });
        }

        var result = await payments.ProcessWebhookAsync(payload, raw, ct);
        logger.LogInformation("Xử lý webhook Reference={Reference}, TxId={TxId}, Amount={Amount}, Processed={Processed}, Result={Result}",
            payload.Reference, payload.TransactionId, payload.Amount, result.Processed, result.Result);

        if (result.TripId.HasValue && result.SeatCodes is not null)
        {
            await hub.Clients.Group(TripSeatsHub.Group(result.TripId.Value)).SendAsync("SeatsChanged", new { seatCodes = result.SeatCodes, status = "Booked" }, ct);
        }

        if (result.BookingId.HasValue)
        {
            await hub.Clients.Group(TripSeatsHub.GroupForBooking(result.BookingId.Value)).SendAsync("PaymentSucceeded", new { bookingId = result.BookingId.Value, bookingCode = result.BookingCode }, ct);
        }
        else
        {
            // Fallback: nếu service chưa trả về BookingId trong Dto thì tra cứu nhanh qua bookingCode nếu có database
            try
            {
                var bookingCode = payload.Reference.StartsWith("STKT", StringComparison.OrdinalIgnoreCase) ? payload.Reference[4..] : payload.Reference;
                var matchedBooking = await db.Bookings.AsNoTracking().SingleOrDefaultAsync(x => x.BookingCode == bookingCode, ct);
                if (matchedBooking != null)
                {
                    await hub.Clients.Group(TripSeatsHub.GroupForBooking(matchedBooking.Id)).SendAsync("PaymentSucceeded", new { bookingId = matchedBooking.Id, bookingCode = matchedBooking.BookingCode }, ct);
                }
            }
            catch (InvalidOperationException)
            {
                // Bỏ qua nếu DbContext không được cấu hình (ví dụ trong mock unit test)
            }
        }

        return Ok(new { success = true, result.Processed, result.Result, reference = payload.Reference });
    }

    private static string ReadJsonString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop)) return "";
        return prop.ValueKind == JsonValueKind.String ? (prop.GetString() ?? "") : "";
    }

    private static string? ReadJsonId(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop)) return null;
        return prop.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrWhiteSpace(prop.GetString()) ? null : prop.GetString(),
            JsonValueKind.Number => prop.GetRawText(),
            _ => null
        };
    }

    private static decimal? ReadJsonDecimal(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var num)) return num;
        if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var parsed)) return parsed;
        return null;
    }

    [HttpGet("state"), AllowAnonymous]
    public async Task<IActionResult> State([FromQuery] string token, CancellationToken ct)
    {
        var state = await payments.GetPaymentStateAsync(token, ct);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpPost("simulate-dev-success"), AllowAnonymous]
    public async Task<IActionResult> SimulateDevSuccess([FromQuery] string token, CancellationToken ct)
    {
        if (!env.IsDevelopment()) return NotFound();
        if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token không hợp lệ.");

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var booking = await db.Bookings.SingleOrDefaultAsync(x => x.LookupTokenHash == tokenHash, ct);
        if (booking is null) return NotFound("Không tìm thấy thông tin lượt đặt.");

        var payment = await db.Payments.SingleOrDefaultAsync(x => x.BookingId == booking.Id, ct);
        if (payment is null) return NotFound("Không tìm thấy thông tin thanh toán.");

        var fakeTxId = "DEV_TX_" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        var payload = new PaymentWebhookPayload(payment.Reference, fakeTxId, payment.Amount, "SUCCESS");
        var rawJson = JsonSerializer.Serialize(payload);

        var result = await payments.ProcessWebhookAsync(payload, rawJson, ct);
        if (result.TripId.HasValue && result.SeatCodes is not null)
            await hub.Clients.Group(TripSeatsHub.Group(result.TripId.Value)).SendAsync("SeatsChanged", new { seatCodes = result.SeatCodes, status = "Booked" }, ct);

        await hub.Clients.Group(TripSeatsHub.GroupForBooking(booking.Id)).SendAsync("PaymentSucceeded", new { bookingId = booking.Id, bookingCode = booking.BookingCode }, ct);

        return Ok(new { success = true, result.Processed, result.Result, redirectUrl = "/tickets/lookup?token=" + Uri.EscapeDataString(token) });
    }
}
