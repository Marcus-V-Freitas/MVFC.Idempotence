namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class InvoicesEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/invoices", async (InvoiceRequest req, CancellationToken ct) =>
        {
            await Task.Delay(50, ct).ConfigureAwait(false);

            var response = new InvoiceResponse(req.ExternalId, req.Value, DateTime.UtcNow);

            return Results.Created($"/api/invoices/{req.ExternalId}", response);
        }).WithIdempotency(
            keyResolver: ctx => ctx.Request.Query["externalId"].FirstOrDefault(),
            ttl: TimeSpan.FromDays(7));
    }
}
