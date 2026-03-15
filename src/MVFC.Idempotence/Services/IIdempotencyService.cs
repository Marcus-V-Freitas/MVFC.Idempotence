namespace MVFC.Idempotence.Services;

public interface IIdempotencyService
{
    public Task<T> ExecuteAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan? ttl = null,
        CancellationToken ct = default);

    public Task<CachedResult?> GetAsync(
        string key,
        CancellationToken ct = default);

    public Task RemoveAsync(
        string key,
        CancellationToken ct = default);

    public Task CacheAsync(
        string key,
        ReadOnlyMemory<byte> payload,
        int statusCode,
        TimeSpan? ttl = null,
        CancellationToken ct = default);
}
