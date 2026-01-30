using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CacheImplementation.Service;

public class CachedRepository
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];
    private readonly ILogger<CachedRepository> _logger;

    public CachedRepository(IDistributedCache cache, ILogger<CachedRepository> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T?>> factory, DistributedCacheEntryOptions options, CancellationToken ct)
    {
        try
        {
            // If found in cache, return it
            var cachedData = await _cache.GetStringAsync(key, ct);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            // Lock per key
            var keyLock = _locks.GetOrAdd(key, new SemaphoreSlim(1, 1));
            await keyLock.WaitAsync(ct);

            try
            {
                // Double-check after acquiring lock
                cachedData = await _cache.GetStringAsync(key, ct);
                if (!string.IsNullOrEmpty(cachedData))
                    return JsonSerializer.Deserialize<T>(cachedData);

                _logger.LogInformation("Cache miss for key: {Key}, loading from factory", key);

                // Load from external source and cache it
                var data = await factory();
                
                if (data != null)
                {
                    await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options, ct);
                    _logger.LogInformation("Data cached for key: {Key}", key);
                }
                else
                {
                    _logger.LogWarning("Factory returned null for key: {Key}", key);
                }

                return data;
            }
            finally
            {
                keyLock.Release();
                if (keyLock.CurrentCount == 1)
                    _locks.TryRemove(key, out _);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Cache operation cancelled for key: {Key}", key);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveAsync(key, ct);
            _logger.LogInformation("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            throw;
        }
    }
}