using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartTicket.Application.Bookings;
using StackExchange.Redis;

namespace SmartTicket.Infrastructure.Bookings;

public sealed class DistributedSeatLockManager : ISeatLockManager
{
    private readonly ILogger<DistributedSeatLockManager> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly bool _allowInMemoryFallback;
    private static readonly ConcurrentDictionary<string, (string Token, DateTime ExpiresAtUtc)> InMemoryLocks = new(StringComparer.OrdinalIgnoreCase);

    public DistributedSeatLockManager(
        IConfiguration configuration,
        ILogger<DistributedSeatLockManager> logger,
        IHostEnvironment environment,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _allowInMemoryFallback = environment.IsDevelopment() && configuration.GetValue<bool>("SeatLocks:AllowInMemoryFallback");
        var redisConn = configuration.GetConnectionString("Redis");
        if (redis is not null)
        {
            _redis = redis;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                try
                {
                    _redis = ConnectionMultiplexer.Connect(redisConn);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, _allowInMemoryFallback
                        ? "Không thể kết nối Redis; Development được phép dùng khóa cục bộ trong tiến trình này."
                        : "Không thể kết nối Redis; khóa ghế sẽ từ chối yêu cầu cho đến khi Redis khả dụng.");
                }
            }
        }
    }

    public async Task<IAsyncDisposable?> TryAcquireLocksAsync(Guid tripId, IReadOnlyList<string> seatCodes, TimeSpan duration, CancellationToken ct)
    {
        if (seatCodes.Count == 0) return new EmptyDisposable();
        var lockToken = Guid.NewGuid().ToString("N");
        var sortedCodes = seatCodes.Select(x => x.Trim().ToUpperInvariant()).Distinct().Order().ToArray();

        if (_redis is not null && _redis.IsConnected)
        {
            return await TryAcquireRedisLocksAsync(tripId, sortedCodes, lockToken, duration, ct);
        }

        if (!_allowInMemoryFallback)
        {
            _logger.LogWarning("Redis được cấu hình nhưng hiện không khả dụng; từ chối khóa ghế để tránh giữ chỗ trùng giữa các instance.");
            return null;
        }

        _logger.LogWarning("Redis không khả dụng; đang dùng khóa trong bộ nhớ cục bộ cho Development.");
        return TryAcquireInMemoryLocks(tripId, sortedCodes, lockToken, duration);
    }

    private async Task<IAsyncDisposable?> TryAcquireRedisLocksAsync(Guid tripId, string[] seatCodes, string lockToken, TimeSpan duration, CancellationToken ct)
    {
        var db = _redis!.GetDatabase();
        var acquiredKeys = new List<string>();

        try
        {
            foreach (var code in seatCodes)
            {
                ct.ThrowIfCancellationRequested();
                var key = $"lock:trip:{tripId:N}:seat:{code}";
                var acquired = await db.StringSetAsync(key, lockToken, duration, When.NotExists);
                if (!acquired)
                {
                    // Fast fail: rollback already acquired keys
                    await ReleaseRedisKeysAsync(db, acquiredKeys, lockToken);
                    return null;
                }
                acquiredKeys.Add(key);
            }

            return new RedisSeatLockReleaser(db, acquiredKeys, lockToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thao tác khóa phân tán Redis; giải phóng và hoàn tác.");
            await ReleaseRedisKeysAsync(db, acquiredKeys, lockToken);
            return null;
        }
    }

    private static IAsyncDisposable? TryAcquireInMemoryLocks(Guid tripId, string[] seatCodes, string lockToken, TimeSpan duration)
    {
        var now = DateTime.UtcNow;
        var acquiredKeys = new List<string>();

        // Purge expired locks first
        foreach (var kvp in InMemoryLocks)
        {
            if (kvp.Value.ExpiresAtUtc <= now)
            {
                InMemoryLocks.TryRemove(kvp.Key, out _);
            }
        }

        lock (InMemoryLocks)
        {
            now = DateTime.UtcNow;
            foreach (var code in seatCodes)
            {
                var key = $"lock:trip:{tripId:N}:seat:{code}";
                if (InMemoryLocks.TryGetValue(key, out var existing) && existing.ExpiresAtUtc > now)
                {
                    // Seat is already locked by another in-flight request -> Fast fail!
                    foreach (var rollbackKey in acquiredKeys)
                    {
                        InMemoryLocks.TryRemove(rollbackKey, out _);
                    }
                    return null;
                }
                InMemoryLocks[key] = (lockToken, now.Add(duration));
                acquiredKeys.Add(key);
            }
        }

        return new InMemorySeatLockReleaser(acquiredKeys, lockToken);
    }

    private static async Task ReleaseRedisKeysAsync(IDatabase db, List<string> keys, string lockToken)
    {
        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        foreach (var key in keys)
        {
            try
            {
                await db.ScriptEvaluateAsync(script, [(RedisKey)key], [(RedisValue)lockToken]);
            }
            catch
            {
                // Ignore rollback errors
            }
        }
    }

    private sealed class RedisSeatLockReleaser(IDatabase db, List<string> keys, string lockToken) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await ReleaseRedisKeysAsync(db, keys, lockToken);
        }
    }

    private sealed class InMemorySeatLockReleaser(List<string> keys, string lockToken) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            lock (InMemoryLocks)
            {
                foreach (var key in keys)
                {
                    if (InMemoryLocks.TryGetValue(key, out var val) && val.Token == lockToken)
                    {
                        InMemoryLocks.TryRemove(key, out _);
                    }
                }
            }
            return ValueTask.CompletedTask;
        }
    }

    private sealed class EmptyDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
