namespace MVFC.Idempotence.Services;

public sealed class IdempotencyService(HybridCache cache, IdempotencyConfig config) : IIdempotencyService
{
    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IdempotencyConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly TimeSpan _minLocalExpiration = TimeSpan.FromMinutes(5);


    private static readonly HybridCacheEntryOptions ReadOnlyOptions = new()
    {
        Flags = HybridCacheEntryFlags.DisableLocalCacheWrite
              | HybridCacheEntryFlags.DisableDistributedCacheWrite,
    };

    public async Task<T> ExecuteAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(operation);

        var cacheKey = BuildKey(key);
        var expiration = ttl ?? _config.Ttl;
        
        var opts = new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = expiration < _minLocalExpiration ? expiration : _minLocalExpiration
        };

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token =>
            {
                var result = await operation(token);
                return new CachedModel<T>(200, result);
            },
            opts,
            cancellationToken: ct).AsTask().ContinueWith(task =>
            {
                var cached = task.Result;
                return cached.IsFailure
                    ? throw new IdempotencyException(cached.Error ?? "Operação anterior falhou.", cached.Status)
                    : cached.Payload!;
            }, ct);
    }

    public async Task<CachedResult?> GetAsync(
        string key, 
        CancellationToken ct = default) => 
            await _cache.GetOrCreateAsync(
                BuildKey(key), _ => ValueTask.FromResult<CachedResult?>(null), ReadOnlyOptions, cancellationToken: ct).AsTask();

    public async Task RemoveAsync(
        string key, 
        CancellationToken ct = default) => 
            await _cache.RemoveAsync(BuildKey(key), ct);

    public async Task CacheAsync(
        string key, 
        ReadOnlyMemory<byte> payload, 
        int statusCode,
        TimeSpan? ttl = null, 
        CancellationToken ct = default) =>
            await PersistAsync(BuildKey(key), new CachedResult(statusCode, payload.ToArray()), ttl ?? _config.Ttl, ct);

    private string BuildKey(string key) => 
        $"{_config.KeyPrefix}{key}";

    private async Task PersistAsync(
        string key,
        CachedResult result,
        TimeSpan expiration,
        CancellationToken ct)
    {
        var opts = new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = expiration < _minLocalExpiration ? expiration : _minLocalExpiration
        };

        await _cache.SetAsync(key, result, opts, cancellationToken: ct);
    }
}