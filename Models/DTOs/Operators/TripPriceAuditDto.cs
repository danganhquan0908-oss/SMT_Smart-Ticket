namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record TripPriceAuditDto(DateTime ChangedAtUtc, string ModifiedBy, decimal OldPrice, decimal NewPrice, string Action);
