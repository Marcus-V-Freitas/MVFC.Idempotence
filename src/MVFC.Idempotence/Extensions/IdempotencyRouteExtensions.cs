namespace MVFC.Idempotence.Extensions;

public static class IdempotencyRouteExtensions
{
    public static TBuilder WithIdempotency<TBuilder>(
        this TBuilder builder,
        TimeSpan? ttl = null,
        IdempotencyOptions? options = null)
            where TBuilder : IEndpointConventionBuilder =>
        builder.WithIdempotency(
            ctx =>
            {
                var config = ctx.RequestServices.GetRequiredService<IdempotencyConfig>();
                var headerName = (options ?? new IdempotencyOptions()).ResolveHeaderName(config);
                return ctx.Request.Headers[headerName].FirstOrDefault();
            },
            ttl,
            options);

    public static TBuilder WithIdempotency<TBuilder>(
        this TBuilder builder,
        Func<HttpContext, string?> keyResolver,
        TimeSpan? ttl = null,
        IdempotencyOptions? options = null)
            where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(new IdempotencyFilter(keyResolver, ttl, options));
        return builder;
    }

    public static TBuilder WithIdempotency<TBuilder>(
        this TBuilder builder,
        string fromRoute,
        TimeSpan? ttl = null,
        IdempotencyOptions? options = null)
            where TBuilder : IEndpointConventionBuilder =>
        builder.WithIdempotency(ctx => ctx.Request.RouteValues[fromRoute]?.ToString(), ttl, options);
}