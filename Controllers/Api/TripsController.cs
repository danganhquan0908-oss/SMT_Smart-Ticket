using BusGoSmartTicket.Hubs;
using BusGoSmartTicket.Models.DTOs.Bookings;
using BusGoSmartTicket.Services.Interfaces.Bookings;
using BusGoSmartTicket.Services.Interfaces.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QRCoder;

namespace BusGoSmartTicket.Controllers.Api;

[ApiController, Route("api/trips")]
public sealed class TripsController(
    IBookingService bookings,
    IPaymentService payments,
    IHubContext<TripSeatsHub> hub) : ControllerBase
{
    [HttpGet("/api/search/trips"), AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] Guid departureStationId, [FromQuery] Guid arrivalStationId, [FromQuery] DateOnly travelDate, CancellationToken ct) =>
        Ok(await bookings.SearchAsync(new TripSearchQuery(departureStationId, arrivalStationId, travelDate), ct));

    [HttpGet("{tripId:guid}/seats"), AllowAnonymous]
    public async Task<IActionResult> Seats(Guid tripId, CancellationToken ct) => Ok(await bookings.GetSeatsAsync(tripId, ct));

    [HttpPost("{tripId:guid}/holds"), Authorize]
    public async Task<IActionResult> Hold(Guid tripId, HoldSeatsRequest request, CancellationToken ct)
    {
        var idStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid? customerUserId = Guid.TryParse(idStr, out var id) ? id : null;
        var hold = await bookings.HoldSeatsAsync(tripId, request with
        {
            CustomerUserId = customerUserId
        }, ct);
        await hub.Clients.Group(TripSeatsHub.Group(tripId)).SendAsync("SeatsChanged", new
        {
            hold.SeatCodes,
            status = "Holding",
            hold.ExpiresAtUtc
        }, ct);

        string? qrImage = null;
        string? qrPayload = null;
        try
        {
            var checkout = await payments.CreateCheckoutAsync(hold.LookupToken, ct);
            if (string.IsNullOrEmpty(checkout.QrPayload))
                throw new InvalidOperationException("Chưa cấu hình ngân hàng.");
            qrPayload = checkout.QrPayload;
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(checkout.QrPayload, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(12);
            qrImage = "data:image/png;base64," + Convert.ToBase64String(png);
        }
        catch
        {
            // Dự phòng nếu lỗi sinh QR thì client dùng fallback
        }

        return Created($"/booking/{hold.BookingCode}", new
        {
            hold.BookingId,
            hold.BookingCode,
            hold.ExpiresAtUtc,
            hold.TotalAmount,
            hold.SeatCodes,
            hold.LookupToken,
            qrPayload,
            qrImage,
            checkoutUrl = $"/payments/checkout?token={Uri.EscapeDataString(hold.LookupToken)}"
        });
    }
}
