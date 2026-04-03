namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments", async (PaymentRequest req, CancellationToken ct) =>
        {
            await Task.Delay(200, ct).ConfigureAwait(false);

            return req.Amount <= 0
                ? Results.UnprocessableEntity(new { error = "Valor deve ser positivo." })
                : Results.Ok(new PaymentResponse(Guid.NewGuid(), req.OrderId, req.Amount, DateTimeOffset.UtcNow));

        }).WithIdempotency(
            ttl: TimeSpan.FromHours(48),
            options: new IdempotencyOptions { HeaderName = "X-Request-Id" });
    }
}
