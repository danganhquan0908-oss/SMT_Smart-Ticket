using BusGoSmartTicket.Data;
using BusGoSmartTicket.Models.DTOs.Account;
using BusGoSmartTicket.Models.Entities;
using BusGoSmartTicket.Services.Interfaces.Account;
using Microsoft.EntityFrameworkCore;
namespace BusGoSmartTicket.Services.Account;

public sealed class EfMembershipService(ApplicationDbContext db) : IMembershipService
{
    private async Task<decimal> SpendAsync(Guid userId, CancellationToken ct) =>
        await db.Payments.Where(p => p.Status == PaymentStatus.Succeeded && p.Booking!.Status == BookingStatus.Paid &&
            p.Booking.CustomerUserId == userId).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
    private async Task<bool> IsCustomerAsync(Guid userId, CancellationToken ct) =>
        await (from ur in db.UserRoles
               join r in db.Roles on ur.RoleId equals r.Id
               where ur.UserId == userId && r.Name == "CUSTOMER"
               select ur).AnyAsync(ct) &&
        !await (from ur in db.UserRoles
                join r in db.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId && r.Name != "CUSTOMER"
                select ur).AnyAsync(ct);

    public async Task<DiscountQuote> QuoteAsync(Guid? userId, decimal price, CancellationToken ct)
    {
        if (userId is null || !await IsCustomerAsync(userId.Value, ct))
            return new(price, price, null, null, 0);
        var tier = MembershipPolicy.TierFor(await SpendAsync(userId.Value, ct));
        var now = DateTime.UtcNow;
        var offer = await db.DiscountCampaigns.AsNoTracking().Where(x => x.IsActive && x.Tier == tier &&
            x.StartsAtUtc <= now && now < x.EndsAtUtc).OrderByDescending(x => x.Percent).ThenBy(x => x.Id).FirstOrDefaultAsync(ct);
        return offer is null ? new(price, price, null, null, 0) :
            new(price, MembershipPolicy.NetSeatPrice(price, offer.Percent), offer.Id, offer.Name, offer.Percent);
    }
    public async Task<MembershipOverview> OverviewAsync(Guid userId, CancellationToken ct)
    {
        var spend = await SpendAsync(userId, ct);
        var tier = MembershipPolicy.TierFor(spend);
        var now = DateTime.UtcNow;
        var offers = await db.DiscountCampaigns.AsNoTracking().Where(x => x.IsActive && x.Tier == tier && x.EndsAtUtc > now)
            .OrderByDescending(x => x.Percent).ThenBy(x => x.Id).ToListAsync(ct);
        var known = await db.MemberNotifications.Where(x => x.UserId == userId).Select(x => x.CampaignId).ToListAsync(ct);
        foreach (var offer in offers.Where(x => !known.Contains(x.Id)))
        {
            // PostgreSQL unique key keeps concurrent page loads idempotent without swallowing unrelated errors.
            var id = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "MemberNotifications" ("Id", "UserId", "CampaignId", "CreatedAtUtc")
                VALUES ({id}, {userId}, {offer.Id}, {now}) ON CONFLICT ("UserId", "CampaignId") DO NOTHING
                """, ct);
        }
        var notices = await db.MemberNotifications.AsNoTracking().Include(x => x.Campaign).Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc).Take(100).ToListAsync(ct);
        var next = MembershipPolicy.Levels.FirstOrDefault(x => x.Spend > spend).Spend;
        return new(spend, tier, next, offers, notices);
    }
    public async Task<bool> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct)
    {
        var notice = await db.MemberNotifications.SingleOrDefaultAsync(x => x.UserId == userId && x.Id == notificationId, ct);
        if (notice is null)
            return false;
        notice.MarkRead();
        await db.SaveChangesAsync(ct);
        return true;
    }
}
