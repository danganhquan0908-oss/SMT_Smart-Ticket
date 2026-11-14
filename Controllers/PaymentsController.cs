using System.Globalization;
using BusGoSmartTicket.Exceptions.Payments;
using BusGoSmartTicket.Models.ViewModels;
using BusGoSmartTicket.Services.Interfaces.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace BusGoSmartTicket.Controllers;

[AllowAnonymous]
public sealed class PaymentsController(IPaymentService payments, IConfiguration configuration) : Controller
{
    [HttpGet("/payments/checkout")]
    public async Task<IActionResult> Checkout(string token, CancellationToken ct)
    {
        try
        {
            var checkout = await payments.CreateCheckoutAsync(token, ct);
            var deepLink = (configuration["Payments:BankAppDeepLink"] ?? "")
                .Replace("{qrPayload}", Uri.EscapeDataString(checkout.QrPayload), StringComparison.Ordinal)
                .Replace("{amount}", Uri.EscapeDataString(checkout.Amount.ToString(CultureInfo.InvariantCulture)), StringComparison.Ordinal)
                .Replace("{reference}", Uri.EscapeDataString("STKT" + checkout.BookingCode), StringComparison.Ordinal);
            return View(new PaymentCheckoutViewModel
            {
                Checkout = checkout,
                QrImage = string.IsNullOrWhiteSpace(checkout.QrPayload) ? null : CreateQrImage(checkout.QrPayload),
                BankAppDeepLink = deepLink,
                Bank = new PaymentBankViewModel
                {
                    Bin = configuration["Payments:BankBin"] ?? "",
                    Name = configuration["Payments:BankName"] ?? "",
                    AccountNumber = configuration["Payments:AccountNumber"] ?? "",
                    AccountName = configuration["Payments:AccountName"] ?? ""
                }
            });
        }
        catch (PaymentNotFoundException) { return NotFound(); }
        catch (PaymentConflictException ex) { return View("CheckoutExpired", new PaymentExpiredViewModel(ex.Message)); }
    }

    [HttpGet("/tickets/lookup")]
    public async Task<IActionResult> Lookup(string token, CancellationToken ct)
    {
        var booking = await payments.GetTicketsAsync(token, ct);
        if (booking is null)
            return NotFound();
        return View("Tickets", new TicketLookupViewModel
        {
            Booking = booking,
            QrImages = booking.Tickets.ToDictionary(t => t.TicketNumber, t => CreateQrImage(t.CheckInJwt))
        });
    }

    private static string CreateQrImage(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        return Convert.ToBase64String(new PngByteQRCode(data).GetGraphic(8));
    }
}
