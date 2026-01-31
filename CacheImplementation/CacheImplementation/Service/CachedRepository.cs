using System.Text.Json;
using CacheImplementation.Model;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheImplementation.Service;

public sealed class CachedRepository(IDistributedCache cache, ILogger<CachedRepository> logger)
{
    private readonly IDistributedCache _cache = cache;
    private readonly KeyedSemaphore _lockManager = new();
    private readonly ILogger<CachedRepository> _logger = logger;

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, DistributedCacheEntryOptions options, CancellationToken ct)
    {
        try
        {
            // 1. Try cache first  
            var cachedData = await _cache.GetStringAsync(key, ct);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedData, CacheJson.Options);
            }

            // 2. Acquire lock
            using var _ = await _lockManager.LockAsync(key, ct);

            // 3. Double-check after acquiring lock
            cachedData = await _cache.GetStringAsync(key, ct);

            if (!string.IsNullOrEmpty(cachedData))
                return JsonSerializer.Deserialize<T>(cachedData, CacheJson.Options);

            _logger.LogInformation("Cache miss for key: {Key}, loading from factory", key);

            // 4. Load from external source and cache it
            var data = await factory();
            if (data == null)
                return default;

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(data, CacheJson.Options), options, ct);
            _logger.LogInformation("Data cached for key: {Key}", key);


            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis failed for key {key}", key);

            // Fallback to source
            return await factory();
        }

    }

    public Task RemoveAsync(string key, CancellationToken ct)
        => _cache.RemoveAsync(key, ct);
}