namespace MVFC.Idempotence.Playground.Api.Responses;

public sealed record InvoiceResponse(
    string ExternalId, 
    decimal Value, 
    DateTime CreatedAt);