namespace BusGoSmartTicket.Models.Entities;

// Thresholds and non-stacking discounts adapted from BusGo's membership workflow.
public static class MembershipPolicy
{
    public static readonly (MemberTier Tier, decimal Spend)[] Levels =
        [(MemberTier.Standard, 0), (MemberTier.Bronze, 500_000), (MemberTier.Silver, 2_000_000),
         (MemberTier.Gold, 5_000_000), (MemberTier.Diamond, 10_000_000)];
    public static MemberTier TierFor(decimal netSpend) => Levels.Last(x => x.Spend <= Math.Max(0, netSpend)).Tier;
    public static decimal NetSeatPrice(decimal price, decimal percent)
    {
        if (price < 0 || percent < 0 || percent > 99)
            throw new ArgumentOutOfRangeException(nameof(percent));
        return price - decimal.Round(price * percent / 100m, 0, MidpointRounding.AwayFromZero);
    }
    public static string Label(MemberTier tier) => tier switch
    {
        MemberTier.Bronze => "Đồng",
        MemberTier.Silver => "Bạc",
        MemberTier.Gold => "Vàng",
        MemberTier.Diamond => "Kim cương",
        _ => "Tiêu chuẩn"
    };
}
