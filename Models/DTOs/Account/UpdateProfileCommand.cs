namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record UpdateProfileCommand(string FullName, string? PhoneNumber);
