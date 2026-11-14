using System.ComponentModel.DataAnnotations;

namespace BusGoSmartTicket.Models.DTOs.Operations;

public sealed class StationMemberRequest
{
    [Required, EmailAddress] public string Email { get; init; } = "";
}
