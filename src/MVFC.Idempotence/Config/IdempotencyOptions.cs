namespace MVFC.Idempotence.Config;

public sealed record IdempotencyOptions(
    string? HeaderName = null,
    ISet<string>? AllowedMethods = null)
{
    internal string ResolveHeaderName(IdempotencyConfig config) =>
        HeaderName ?? config.HeaderName;

    internal ISet<string> ResolveAllowedMethods(IdempotencyConfig config) =>
        AllowedMethods ?? config.AllowedMethods;
}