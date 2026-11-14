namespace BusGoSmartTicket.Models.DTOs.Operators;

public sealed record ActivationInfo(string Email, string CompanyName, DateTime ExpiresAtUtc);
