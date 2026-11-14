namespace BusGoSmartTicket.Models.Entities;

// Thresholds and non-stacking discounts adapted from BusGo's membership workflow.
public sealed class MemberNotification
{
    private MemberNotification()
    {
    }
    public MemberNotification(Guid userId, Guid campaignId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CampaignId = campaignId;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid UserId
    {
        get; private set;
    }
    public Guid CampaignId
    {
        get; private set;
    }
    public DiscountCampaign? Campaign
    {
        get; private set;
    }
    public DateTime CreatedAtUtc
    {
        get; private set;
    }
    public DateTime? ReadAtUtc
    {
        get; private set;
    }
    public void MarkRead() => ReadAtUtc ??= DateTime.UtcNow;
}
