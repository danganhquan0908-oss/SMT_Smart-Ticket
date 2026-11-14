namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record PayoutResult(bool Succeeded, string? ProviderReference, string? Error);
