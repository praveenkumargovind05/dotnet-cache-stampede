using System;
using CacheImplementation.Repository;
using CacheImplementation.Repository.Dapper;
using CacheImplementation.Service;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace CacheImplementation.ServiceExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IDataAccess, DataAccess>();
        services.AddSingleton<CachedRepository>();
        return services;
    }
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var redisConn = builder.Configuration.GetValue<string>("Redis:ConnectionString")!;
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConn);

            options.AbortOnConnectFail = false;
            options.ConnectRetry = 5;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;
            options.KeepAlive = 60;
            options.ReconnectRetryPolicy = new ExponentialRetry(5000);

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConn;
            opt.InstanceName = builder.Configuration["Redis:InstanceName"];
        });
        
        return services;
    }
}
