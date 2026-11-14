namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record CustomerProfileDto(Guid Id, string FullName, string Email, string? PhoneNumber);
