namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed record SettlementDto(Guid Id, Guid TripId, string Operator, decimal GrossAmount, decimal CommissionAmount, decimal NetAmount, string Status, string? ProviderReference, DateTime CreatedAtUtc, DateTime? PaidAtUtc);
