using Microsoft.AspNetCore.Identity;

namespace BusGoSmartTicket.Models.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName
    {
        get; set;
    }
    public bool IsActive
    {
        get; set;
    }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
