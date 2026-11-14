namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record PayoutInstruction(Guid SettlementId, string Reference, decimal Amount, string BankBin, string AccountNumber, string AccountName);
