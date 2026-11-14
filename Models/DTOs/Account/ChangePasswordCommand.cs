namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword);
