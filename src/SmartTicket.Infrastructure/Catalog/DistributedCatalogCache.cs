using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartTicket.Application.Catalog;

namespace SmartTicket.Infrastructure.Catalog;

public sealed class DistributedCatalogCache(
    IDistributedCache cache,
    ILogger<DistributedCatalogCache>? logger = null) : ICatalogCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        try
        {
            var value = await cache.GetStringAsync(key, ct);
            return value is null ? default : JsonSerializer.Deserialize<T>(value, JsonOptions);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Không thể đọc dữ liệu từ cache cho key '{Key}'. Tự động truy vấn trực tiếp từ database.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        try
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(value, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Không thể ghi dữ liệu vào cache cho key '{Key}'. Bỏ qua lưu cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        try
        {
            await cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Không thể xóa cache cho key '{Key}'. Bỏ qua xóa cache.", key);
        }
    }
}
