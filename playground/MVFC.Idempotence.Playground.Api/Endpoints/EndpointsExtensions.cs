namespace MVFC.Idempotence.Playground.Api.Endpoints;

public static class EndpointsExtensions
{
    public static void MapDefaultEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapOrderEndpoints();
        app.MapPaymentsEndpoints();
        app.MapInvoiceEndpoints();
        app.MapShippingEndpoints();
        app.MapIdempotencyEndpoints();
    }
}
