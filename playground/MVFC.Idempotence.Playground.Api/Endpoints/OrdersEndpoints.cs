namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest req, CancellationToken ct) =>
        {
            await Task.Delay(100, ct);
            var order = new OrderCreatedResponse(Guid.NewGuid(), req.ProductId, req.Quantity, DateTime.UtcNow);
            return Results.Created($"/api/orders/{order.OrderId}", order);
        }).WithIdempotency();

        app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest req, CancellationToken ct) =>
        {
            await Task.Delay(50, ct);
            return Results.Ok(new OrderUpdatedResponse(orderId, req.Status, DateTime.UtcNow));
        }).WithIdempotency(fromRoute: "orderId");
    }
}
