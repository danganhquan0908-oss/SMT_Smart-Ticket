using BusGoSmartTicket.Models.Entities;
namespace BusGoSmartTicket.Models.DTOs.Account;

public sealed record MembershipOverview(decimal NetSpend, MemberTier Tier, decimal NextThreshold,
    IReadOnlyList<DiscountCampaign> Offers, IReadOnlyList<MemberNotification> Notifications);
