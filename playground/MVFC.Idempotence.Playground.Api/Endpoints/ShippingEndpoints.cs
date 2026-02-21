namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class ShippingEndpoints
{
    public static void MapShippingEndpoints(this IEndpointRouteBuilder app)
    {
        var shipping = app.MapGroup("/api/shipping")
            .WithIdempotency(options: new IdempotencyOptions
            {
                AllowedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POST" }
            });

        shipping.MapPost("/dispatch", async (DispatchRequest req, CancellationToken ct) =>
        {
            await Task.Delay(100, ct);
            return Results.Ok(new ShippingResponse(Guid.NewGuid(), req.OrderId, "dispatched", DateTime.UtcNow));
        });

        shipping.MapPost("/cancel", async (CancelRequest req, CancellationToken ct) =>
        {
            await Task.Delay(100, ct);
            return Results.Ok(new ShippingResponse(Guid.NewGuid(), req.OrderId, "cancelled", DateTime.UtcNow));
        });
    }
}