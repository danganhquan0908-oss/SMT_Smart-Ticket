using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class DispatcherInviteForm
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
}
