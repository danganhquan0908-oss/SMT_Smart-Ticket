using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SmartTicket.Application.Payments;

namespace SmartTicket.Web.Controllers;

[AllowAnonymous]
public sealed class PaymentsController(IPaymentService payments, IConfiguration configuration, IHostEnvironment environment) : Controller
{
    [HttpGet("/payments/checkout")]
    public async Task<IActionResult> Checkout(string token, CancellationToken ct)
    {
        try
        {
            var model = await payments.CreateCheckoutAsync(token, ct);
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["Cache-Control"] = "no-store";
            
            try
            {
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(model.QrPayload, QRCodeGenerator.ECCLevel.Q);
                var png = new PngByteQRCode(data).GetGraphic(12);
                ViewBag.QrImage = Convert.ToBase64String(png);
            }
            catch
            {
                ViewBag.QrImage = null;
            }

            ViewBag.Token = token;
            ViewBag.IsDev = environment.IsDevelopment();
            ViewBag.BankBin = configuration["Payments:BankBin"] ?? "";
            ViewBag.BankName = configuration["Payments:BankName"] ?? "";
            ViewBag.AccountNumber = configuration["Payments:AccountNumber"] ?? "";
            ViewBag.AccountName = configuration["Payments:AccountName"] ?? "";
            var deepLink = configuration["Payments:BankAppDeepLink"] ?? "";
            ViewBag.BankAppDeepLink = deepLink
                .Replace("{qrPayload}", Uri.EscapeDataString(model.QrPayload), StringComparison.Ordinal)
                .Replace("{amount}", Uri.EscapeDataString(model.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)), StringComparison.Ordinal)
                .Replace("{reference}", Uri.EscapeDataString("STKT" + model.BookingCode), StringComparison.Ordinal);
            return View("~/Views/Payments/Checkout.cshtml", model);
        }
        catch (PaymentNotFoundException) { return NotFound(); }
        catch (PaymentConflictException ex) { ViewBag.Error = ex.Message; return View("~/Views/Payments/CheckoutExpired.cshtml"); }
        catch (Exception ex) { ViewBag.Error = ex.Message; return View("~/Views/Payments/CheckoutExpired.cshtml"); }
    }

    [HttpGet("/tickets/lookup")]
    public async Task<IActionResult> Lookup(string token, CancellationToken ct)
    {
        var tickets = await payments.GetTicketsAsync(token, ct);
        if (tickets is null) return NotFound();
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Cache-Control"] = "no-store";
        ViewBag.Token = token;
        ViewBag.TicketQr = tickets.Tickets.ToDictionary(x => x.TicketNumber, x =>
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(x.CheckInJwt, QRCodeGenerator.ECCLevel.Q);
            return Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(8));
        });
        return View("~/Views/Payments/Tickets.cshtml", tickets);
    }
}
