namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record DispatchResult(bool Approved, string Status, string Signature, DateTime SignedAtUtc);
