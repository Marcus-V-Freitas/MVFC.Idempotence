namespace MVFC.Idempotence.Playground.Api.Responses;

public sealed record ShippingResponse(
    Guid TrackingId, 
    Guid OrderId, 
    string Status, 
    DateTime ProcessedAt);