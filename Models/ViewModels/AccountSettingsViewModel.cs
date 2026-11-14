namespace BusGoSmartTicket.Models.ViewModels;

/// <summary>Current-user account data and the two independent account forms.</summary>
public sealed class AccountSettingsViewModel
{
    public CustomerProfileViewModel Profile { get; init; } = new();
    public ChangePasswordViewModel Password { get; init; } = new();
    public string DisplayName { get; init; } = "";
    public string Email { get; init; } = "";
    public string Initials { get; init; } = "";
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyList<string> RoleLabels { get; init; } = [];
    public bool TwoFactorEnabled { get; init; }
    public bool HasPassword { get; init; }
    public bool IsCustomer { get; init; }
    public string ActiveSection { get; init; } = "profile";
    public string WorkspaceUrl { get; init; } = "/";
    public string WorkspaceLabel { get; init; } = "Tìm chuyến xe";
}
