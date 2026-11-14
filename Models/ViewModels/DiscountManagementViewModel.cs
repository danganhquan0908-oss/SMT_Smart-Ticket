using BusGoSmartTicket.Models.Entities;

namespace BusGoSmartTicket.Models.ViewModels;

public sealed class DiscountManagementViewModel
{
    public DiscountForm Form { get; init; } = new();
    public IReadOnlyList<DiscountCampaign> Campaigns { get; init; } = [];
}
