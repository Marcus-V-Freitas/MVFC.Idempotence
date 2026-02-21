namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class InvoicesEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/invoices", async (InvoiceRequest req, CancellationToken ct) =>
        {
            await Task.Delay(50, ct);
            return Results.Created($"/api/invoices/{req.ExternalId}",
                new InvoiceResponse(req.ExternalId, req.Value, DateTime.UtcNow));
        }).WithIdempotency(
            keyResolver: ctx => ctx.Request.Query["externalId"].FirstOrDefault(),
            ttl: TimeSpan.FromDays(7));
    }
}