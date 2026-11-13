using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SmartTicket.Infrastructure.Bookings;
using StackExchange.Redis;
using Xunit.Sdk;

namespace SmartTicket.Tests;

public sealed class RedisSeatLockIntegrationTests
{
    [Fact]
    public async Task SeparateManagers_UseSharedRedisLock_AndReleaseItSafely()
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTTICKET_TEST_REDIS")
            ?? "127.0.0.1:6379,abortConnect=false,connectRetry=0,connectTimeout=1000";
        IConnectionMultiplexer firstConnection;
        IConnectionMultiplexer secondConnection;
        try
        {
            firstConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            secondConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        }
        catch (RedisException)
        {
            throw SkipException.ForSkip("Redis integration test skipped: set SMARTTICKET_TEST_REDIS or start Redis at localhost:6379.");
        }

        using (firstConnection)
        using (secondConnection)
        {
            try
            {
                await firstConnection.GetDatabase().PingAsync();
                await secondConnection.GetDatabase().PingAsync();
            }
            catch (RedisException)
            {
                throw SkipException.ForSkip("Redis integration test skipped: Redis is not accepting connections.");
            }

            var configuration = new ConfigurationBuilder().Build();
            var logger = NullLogger<DistributedSeatLockManager>.Instance;
            var environment = new TestHostEnvironment(Environments.Production);
            var firstManager = new DistributedSeatLockManager(configuration, logger, environment, firstConnection);
            var secondManager = new DistributedSeatLockManager(configuration, logger, environment, secondConnection);
            var tripId = Guid.NewGuid();

            var firstLock = await firstManager.TryAcquireLocksAsync(
                tripId, ["A01"], TimeSpan.FromSeconds(10), CancellationToken.None);
            Assert.NotNull(firstLock);
            await using (firstLock)
            {
                var competingLock = await secondManager.TryAcquireLocksAsync(
                    tripId, ["A01"], TimeSpan.FromSeconds(10), CancellationToken.None);
                Assert.Null(competingLock);
            }

            await using var lockAfterRelease = await secondManager.TryAcquireLocksAsync(
                tripId, ["A01"], TimeSpan.FromSeconds(10), CancellationToken.None);
            Assert.NotNull(lockAfterRelease);
        }
    }
}
