namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest req, CancellationToken ct) =>
        {
            await Task.Delay(100, ct).ConfigureAwait(false);

            var response = new OrderCreatedResponse(Guid.NewGuid(), req.ProductId, req.Quantity, DateTime.UtcNow);

            return Results.Created($"/api/orders/{response.OrderId}", response);
        }).WithIdempotency();

        app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest req, CancellationToken ct) =>
        {
            await Task.Delay(50, ct).ConfigureAwait(false);

            var response = new OrderUpdatedResponse(orderId, req.Status, DateTime.UtcNow);

            return Results.Ok(response);
        }).WithIdempotency(fromRoute: "orderId");
    }
}
