namespace MVFC.Idempotence.Filters;

internal sealed class IdempotencyFilter(
    Func<HttpContext, string?> keyResolver,
    TimeSpan? ttl = null,
    IdempotencyOptions? options = null) : IEndpointFilter
{
    private readonly Func<HttpContext, string?> _keyResolver = keyResolver;
    private readonly TimeSpan? _ttl = ttl;
    private readonly IdempotencyOptions _options = options ?? new IdempotencyOptions();
    private static readonly RecyclableMemoryStreamManager _streamManager = new();
    private FilterState? _state;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpCtx = context.HttpContext;

        var state = _state;
        if (state is null)
        {
            var config = httpCtx.RequestServices.GetRequiredService<IdempotencyConfig>();
            state = new FilterState(
                _options.ResolveAllowedMethods(config),
                _options.ResolveHeaderName(config)
            );
            _state = state;
        }

        if (!state.AllowedMethods.Contains(httpCtx.Request.Method))
            return await next(context).ConfigureAwait(false);

        var key = _keyResolver(httpCtx);
        if (string.IsNullOrWhiteSpace(key))
            return Results.BadRequest(new { error = $"Header '{state.HeaderName}' ausente ou inválido." });

        var service = httpCtx.RequestServices.GetRequiredService<IIdempotencyService>();
        var cached = await service.GetAsync(key, httpCtx.RequestAborted).ConfigureAwait(false);

        if (cached is not null)
        {
            return cached.IsFailure
                ? Results.Problem(cached.Error, statusCode: cached.Status)
                : new RawJsonResult(cached.Payload ?? [], cached.Status);
        }

        var result = await next(context).ConfigureAwait(false);
        await TryCacheResultAsync(service, key, result, httpCtx).ConfigureAwait(false);

        return result;
    }

    private async Task TryCacheResultAsync(
        IIdempotencyService service,
        string key,
        object? handlerResult,
        HttpContext ctx)
    {
        try
        {
            using var buffer = _streamManager.GetStream();
            var buffered = new BufferedHttpResponse(ctx, buffer);

            if (handlerResult is IResult result)
                await result.ExecuteAsync(buffered.Context).ConfigureAwait(false);

            var statusCode = buffered.Context.Response.StatusCode;
            if (statusCode is < 200 or >= 300)
                return;

            buffer.Position = 0;
            var payload = new byte[buffer.Length];
            await buffer.ReadExactlyAsync(payload, ctx.RequestAborted).ConfigureAwait(false);

            await service.CacheAsync(key, payload, statusCode, _ttl, ctx.RequestAborted).ConfigureAwait(false);
        }
        catch
        {
            // Nunca quebrar o fluxo principal por falha no cache
        }
    }
}
