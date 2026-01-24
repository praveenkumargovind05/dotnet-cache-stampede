using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheStampede.Services;

public class CachedRepository(IDistributedCache cache)
{
    private readonly IDistributedCache _cache = cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = [];

    public async Task<T?> GetOrLoadAsync<T>(string cacheKey, Func<Task<T?>> dbFactory, TimeSpan ttl, CancellationToken ct = default)
    {
        // 1. Happy path
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedValue))
            return JsonSerializer.Deserialize<T?>(cachedValue);

        // 2. Lock per key (only one loader allowed to get data from database)
        var keyLock = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(ct);

        try
        {
            // 3. Check once again after acquiring lock
            cachedValue = await _cache.GetStringAsync(cacheKey, ct);
            if (!string.IsNullOrEmpty(cachedValue))
                return JsonSerializer.Deserialize<T?>(cachedValue);

            // 4. Load from Database
            var data = await dbFactory();

            // 5. Cache it
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, ct);

            return data;
        }
        finally
        {
            keyLock.Release();
            if (keyLock.CurrentCount == 1)
                _locks.TryRemove(cacheKey, out _);
            // When a thread acquires it: WaitAsync() → count becomes 0
            // When we release it: Release() → count becomes 1
        }
    }

}
