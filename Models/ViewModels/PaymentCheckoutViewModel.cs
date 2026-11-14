using BusGoSmartTicket.Models.DTOs.Payments;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class PaymentCheckoutViewModel
{
    public required PaymentCheckoutDto Checkout
    {
        get; init;
    }
    public required PaymentBankViewModel Bank
    {
        get; init;
    }
    public string? QrImage
    {
        get; init;
    }
    public string BankAppDeepLink { get; init; } = "";
}
