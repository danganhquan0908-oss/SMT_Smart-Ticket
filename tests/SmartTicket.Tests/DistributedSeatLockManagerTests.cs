using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SmartTicket.Infrastructure.Bookings;

namespace SmartTicket.Tests;

public sealed class DistributedSeatLockManagerTests
{
    [Fact]
    public async Task RedisNotConfigured_UsesProcessLocalLockForDevelopment()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SeatLocks:AllowInMemoryFallback"] = "true" })
            .Build();
        var manager = new DistributedSeatLockManager(
            configuration,
            NullLogger<DistributedSeatLockManager>.Instance,
            new TestHostEnvironment(Environments.Development));

        await using var held = await manager.TryAcquireLocksAsync(
            Guid.NewGuid(), ["A01"], TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.NotNull(held);
    }

    [Fact]
    public async Task RedisConfiguredButUnavailable_FailsClosedInsteadOfUsingLocalLock()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Redis"] = "localhost:1,abortConnect=false,connectRetry=0,connectTimeout=300"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var manager = new DistributedSeatLockManager(
            configuration,
            NullLogger<DistributedSeatLockManager>.Instance,
            new TestHostEnvironment(Environments.Production));

        var held = await manager.TryAcquireLocksAsync(
            Guid.NewGuid(), ["A01"], TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.Null(held);
    }

    [Fact]
    public async Task RedisConfiguredButUnavailable_UsesLocalLockOnlyWhenDevelopmentFallbackIsEnabled()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Redis"] = "localhost:1,abortConnect=false,connectRetry=0,connectTimeout=300",
            ["SeatLocks:AllowInMemoryFallback"] = "true"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var manager = new DistributedSeatLockManager(
            configuration,
            NullLogger<DistributedSeatLockManager>.Instance,
            new TestHostEnvironment(Environments.Development));

        await using var held = await manager.TryAcquireLocksAsync(
            Guid.NewGuid(), ["A01"], TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.NotNull(held);
    }
}
