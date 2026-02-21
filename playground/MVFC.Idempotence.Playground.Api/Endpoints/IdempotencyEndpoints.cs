namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class IdempotencyEndpoints
{
    public static void MapIdempotencyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/idempotency/{key}", async (string key, IIdempotencyService svc, CancellationToken ct) =>
        {
            await svc.RemoveAsync(key, ct);
            return Results.NoContent();
        });
    }
}