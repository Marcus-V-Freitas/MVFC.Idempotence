namespace MVFC.Idempotence.Services;

public sealed class IdempotencyService(HybridCache cache, IdempotencyConfig config) : IIdempotencyService
{
    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IdempotencyConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly TimeSpan _minLocalExpiration = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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
        var cached = await ReadAsync(cacheKey, ct);

        if (cached is not null)
        {
            var resolved = ResolveCached<T>(cached);
            return resolved.IsFailed ? 
                throw new IdempotencyException(resolved.Errors[0].Message, cached.Status) : 
                resolved.Value;
        }

        var result = await operation(ct);

        var serialized = Serialize(result);
        if (serialized.IsFailed)
            throw new InvalidOperationException(serialized.Errors[0].Message);

        await PersistAsync(cacheKey, new CachedResult(Status: 200, Payload: serialized.Value), ttl ?? _config.Ttl, ct);

        return result;
    }

    public Task<CachedResult?> GetAsync(
        string key, 
        CancellationToken ct = default) => 
            ReadAsync(BuildKey(key), ct);

    public Task RemoveAsync(
        string key, 
        CancellationToken ct = default) => 
            _cache.RemoveAsync(BuildKey(key), ct).AsTask();

    public Task CacheAsync(
        string key, 
        string payload, 
        int statusCode,
        TimeSpan? ttl = null, 
        CancellationToken ct = default) =>
            PersistAsync(BuildKey(key), new CachedResult(statusCode, payload), ttl ?? _config.Ttl, ct);

    private string BuildKey(string key) => 
        $"{_config.KeyPrefix}{key}";

    private async Task<CachedResult?> ReadAsync(
        string cacheKey, 
        CancellationToken ct) => 
            await _cache.GetOrCreateAsync<CachedResult?>(
                cacheKey, _ => ValueTask.FromResult<CachedResult?>(null), ReadOnlyOptions, cancellationToken: ct);

    private Task PersistAsync(
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

        return _cache.SetAsync(key, result, opts, cancellationToken: ct).AsTask();
    }

    private static Result<T> ResolveCached<T>(CachedResult cached) => 
        cached.IsFailure ? 
            Result.Fail<T>(cached.Error ?? "Operação anterior falhou.") : 
            Deserialize<T>(cached.Payload!);

    private static Result<string> Serialize<T>(T value)
    {
        try
        {
            return Result.Ok(JsonSerializer.Serialize(value, JsonOpts));
        }
        catch (JsonException ex)
        {
            return Result.Fail<string>($"Erro ao serializar payload: {ex.Message}");
        }
    }

    private static Result<T> Deserialize<T>(string json)
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(json, JsonOpts);
            return value is null
                ? Result.Fail<T>("Payload desserializado é nulo.")
                : Result.Ok(value);
        }
        catch (JsonException ex)
        {
            return Result.Fail<T>($"Erro ao desserializar payload: {ex.Message}");
        }
    }
}