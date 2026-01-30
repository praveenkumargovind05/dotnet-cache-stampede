using System;
using CacheImplementation.Repository;
using CacheImplementation.Service;
using Microsoft.Extensions.Caching.Memory;

namespace CacheImplementation.ServiceExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDataAccess, DataAccess>();
        services.AddSingleton<CachedRepository>();
        return services;
    }
    public static IServiceCollection AddDistributedCache(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.Configure<MemoryDistributedCacheOptions>(options =>
        {
            // options.SizeLimit = 100; // Max number of entries
            // options.CompactionPercentage = 0.25; // Remove 25% when limit hit
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
        });
        return services;
    }

    public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.Configure<MemoryCacheOptions>(options =>
        {
            // options.SizeLimit = 100; // Max number of entries
            // options.CompactionPercentage = 0.25; // Remove 25% when limit hit
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
        });
        return services;
    }
}
