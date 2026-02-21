namespace MVFC.Idempotence.Extensions;

public static class IdempotencyServiceCollectionExtensions
{
    public static IServiceCollection AddIdempotencyMemory(
        this IServiceCollection services,
        Action<IdempotencyConfig>? configure = null) =>
            services.AddIdempotencyCore(configure);

    public static IServiceCollection AddIdempotencyRedis(
        this IServiceCollection services,
        string connectionString,
        Action<IdempotencyConfig>? configure = null)
    {
        services.AddStackExchangeRedisCache(opts => opts.Configuration = connectionString);
        return services.AddIdempotencyCore(configure);
    }

    private static IServiceCollection AddIdempotencyCore(
        this IServiceCollection services,
        Action<IdempotencyConfig>? configure)
    {
        var config = new IdempotencyConfig();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddHybridCache(opts =>
        {
            opts.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = config.Ttl,
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });

        services.AddScoped<IIdempotencyService, IdempotencyService>();
        return services;
    }
}