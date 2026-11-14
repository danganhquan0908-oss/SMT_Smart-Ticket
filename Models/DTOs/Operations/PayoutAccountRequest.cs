using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed class PayoutAccountRequest
{
    [Required, RegularExpression("^[0-9]{6}$")] public string BankBin { get; init; } = "";
    [Required, StringLength(28, MinimumLength = 5)] public string AccountNumber { get; init; } = "";
    [Required, StringLength(100, MinimumLength = 2)] public string AccountName { get; init; } = "";
}
