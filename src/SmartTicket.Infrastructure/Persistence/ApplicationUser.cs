using Microsoft.AspNetCore.Identity;

namespace SmartTicket.Infrastructure.Persistence;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
