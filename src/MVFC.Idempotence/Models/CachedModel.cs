namespace MVFC.Idempotence.Models;

public sealed record CachedModel<T>(
    [property: JsonPropertyName("s")] int Status,
    [property: JsonPropertyName("p")] T? Payload = default,
    [property: JsonPropertyName("e")] string? Error = null)
{
    [JsonPropertyName("t")]
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    public bool IsSuccess => Status is >= 200 and < 300;

    public bool IsFailure => !IsSuccess;
}
