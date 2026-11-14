namespace BusGoSmartTicket.Models.ViewModels;

public sealed class PaymentBankViewModel
{
    public string Bin { get; init; } = "";
    public string Name { get; init; } = "";
    public string AccountNumber { get; init; } = "";
    public string AccountName { get; init; } = "";
}
