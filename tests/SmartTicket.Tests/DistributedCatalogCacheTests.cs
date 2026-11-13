using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using SmartTicket.Infrastructure.Catalog;

namespace SmartTicket.Tests;

public sealed class DistributedCatalogCacheTests
{
    private sealed record CachedRouteItem(string Code, int SeatCount);

    [Fact]
    public async Task SetGetRemove_WhenCacheIsAvailable_RoundTripsAndDeletes()
    {
        var innerCache = new InMemoryFakeDistributedCache();
        var cache = new DistributedCatalogCache(innerCache, NullLogger<DistributedCatalogCache>.Instance);
        var key = "catalog:routes:test";
        var item = new CachedRouteItem("SG-DL", 34);

        await cache.SetAsync(key, item, TimeSpan.FromMinutes(5), CancellationToken.None);
        var loaded = await cache.GetAsync<CachedRouteItem>(key, CancellationToken.None);

        Assert.Equal(item, loaded);

        await cache.RemoveAsync(key, CancellationToken.None);
        var afterRemove = await cache.GetAsync<CachedRouteItem>(key, CancellationToken.None);

        Assert.Null(afterRemove);
    }

    [Fact]
    public async Task GetSetRemove_WhenUnderlyingCacheThrows_DegradesGracefullyWithoutThrowing()
    {
        var cache = new DistributedCatalogCache(
            new ThrowingDistributedCache(),
            NullLogger<DistributedCatalogCache>.Instance);

        var loaded = await cache.GetAsync<CachedRouteItem>("catalog:stations:active:v1", CancellationToken.None);
        await cache.SetAsync("catalog:stations:active:v1", new CachedRouteItem("HN-HP", 22), TimeSpan.FromMinutes(1), CancellationToken.None);
        await cache.RemoveAsync("catalog:stations:active:v1", CancellationToken.None);

        Assert.Null(loaded);
    }

    private sealed class InMemoryFakeDistributedCache : IDistributedCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _store = new();

        public byte[]? Get(string key) => _store.TryGetValue(key, out var val) ? val : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _store[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _store.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDistributedCache : IDistributedCache
    {
        private static Exception Failure() => new InvalidOperationException("Redis connection timed out.");

        public byte[]? Get(string key) => throw Failure();
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromException<byte[]?>(Failure());
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Failure();
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => Task.FromException(Failure());
        public void Refresh(string key) => throw Failure();
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.FromException(Failure());
        public void Remove(string key) => throw Failure();
        public Task RemoveAsync(string key, CancellationToken token = default) => Task.FromException(Failure());
    }
}
