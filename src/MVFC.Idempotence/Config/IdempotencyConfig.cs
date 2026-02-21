namespace MVFC.Idempotence.Config;

public sealed record IdempotencyConfig
{
    public TimeSpan Ttl { get; set; } = TimeSpan.FromHours(24);
    
    public string KeyPrefix { get; set; } = "idempotency:";
    
    public string HeaderName { get; set; } = "Idempotency-Key";

    public ISet<string> AllowedMethods { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH"
    };
}