namespace MVFC.Idempotence.Services;

public interface IIdempotencyService
{
    Task<T> ExecuteAsync<T>(
        string key, 
        Func<CancellationToken, Task<T>> operation, 
        TimeSpan? ttl = null, 
        CancellationToken ct = default);
    
    Task<CachedResult?> GetAsync(
        string key, 
        CancellationToken ct = default);
    
    Task RemoveAsync(
        string key, 
        CancellationToken ct = default);
 
    Task CacheAsync(
        string key, 
        ReadOnlyMemory<byte> payload, 
        int statusCode, 
        TimeSpan? ttl = null, 
        CancellationToken ct = default);
}