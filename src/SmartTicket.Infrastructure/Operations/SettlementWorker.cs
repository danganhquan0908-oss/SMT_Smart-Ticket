using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTicket.Application.Operations;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;

namespace SmartTicket.Infrastructure.Operations;

public sealed class SettlementWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<SettlementWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try { await RunCycleAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "Settlement cycle failed."); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartTicketDbContext>();
        var rate = decimal.TryParse(configuration["Settlement:CommissionRate"], System.Globalization.CultureInfo.InvariantCulture, out var configuredRate) ? configuredRate : 0.05m;
        if (rate is < 0 or > 1) throw new InvalidOperationException("Settlement:CommissionRate phải từ 0 đến 1.");
        var completed = await db.Trips.Include(x => x.Tickets)
            .Where(x => x.Status == TripStatus.Completed && !db.TripSettlements.Any(s => s.TripId == x.Id) && db.DispatchClearances.Any(c => c.TripId == x.Id && c.IsApproved))
            .OrderBy(x => x.DepartureTimeUtc).Take(100).ToListAsync(ct);
        foreach (var trip in completed)
        {
            var gross = trip.Tickets.Where(t => t.Status is TicketLifecycleStatus.Sold or TicketLifecycleStatus.CheckedIn).Sum(t => t.PriceAtPurchase);
            if (gross <= 0) continue;
            var commission = decimal.Round(gross * rate, 2, MidpointRounding.AwayFromZero);
            db.TripSettlements.Add(new TripSettlement(trip.Id, trip.OperatorId, gross, commission));
        }
        if (completed.Count > 0) await db.SaveChangesAsync(ct);

        var provider = scope.ServiceProvider.GetRequiredService<ISettlementPayoutProvider>();
        if (!provider.IsConfigured) return;
        var now = DateTime.UtcNow;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var queue = await db.TripSettlements.FromSqlInterpolated($"SELECT * FROM \"TripSettlements\" WHERE \"Status\" IN ('Pending','Failed') OR (\"Status\" = 'Processing' AND \"ProcessingAtUtc\" <= {now.AddMinutes(-5)}) ORDER BY \"CreatedAtUtc\" LIMIT 50 FOR UPDATE SKIP LOCKED").ToListAsync(ct);
        foreach (var settlement in queue) settlement.MarkProcessing();
        if (queue.Count == 0) { await tx.CommitAsync(ct); return; }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        foreach (var settlement in queue)
        {
            var op = await db.BusOperators.SingleAsync(x => x.Id == settlement.OperatorId, ct);
            PayoutResult result;
            if (string.IsNullOrWhiteSpace(op.PayoutBankBin) || string.IsNullOrWhiteSpace(op.PayoutAccountNumber) || string.IsNullOrWhiteSpace(op.PayoutAccountName))
                result = new PayoutResult(false, null, "Nhà xe chưa khai báo tài khoản nhận tiền.");
            else
                result = await provider.TransferAsync(new PayoutInstruction(settlement.Id, $"STT-{settlement.TripId:N}", settlement.NetAmount, op.PayoutBankBin, op.PayoutAccountNumber, op.PayoutAccountName), ct);
            if (result.Succeeded) settlement.MarkPaid(result.ProviderReference!);
            else settlement.MarkFailed(result.Error ?? "Payout không thành công.");
            await db.SaveChangesAsync(ct);
        }
    }
}
