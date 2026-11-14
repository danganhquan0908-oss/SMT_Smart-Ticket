namespace BusGoSmartTicket.Models.Entities;

// Thresholds and non-stacking discounts adapted from BusGo's membership workflow.
public sealed class DiscountCampaign
{
    private DiscountCampaign()
    {
    }
    public DiscountCampaign(string name, MemberTier tier, decimal percent, DateTime startsAtUtc, DateTime endsAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100 || !Enum.IsDefined(tier) ||
            percent < .01m || percent > 99 || decimal.Round(percent, 2) != percent ||
            startsAtUtc.Kind != DateTimeKind.Utc || endsAtUtc.Kind != DateTimeKind.Utc || endsAtUtc <= startsAtUtc)
            throw new ArgumentException("Tên, hạng, mức giảm hoặc thời gian ưu đãi không hợp lệ.");
        Id = Guid.NewGuid();
        Name = name.Trim();
        Tier = tier;
        Percent = percent;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        IsActive = true;
    }
    public Guid Id
    {
        get; private set;
    }
    public string Name { get; private set; } = "";
    public MemberTier Tier
    {
        get; private set;
    }
    public decimal Percent
    {
        get; private set;
    }
    public DateTime StartsAtUtc
    {
        get; private set;
    }
    public DateTime EndsAtUtc
    {
        get; private set;
    }
    public bool IsActive
    {
        get; private set;
    }
    public void Deactivate() => IsActive = false;
}
