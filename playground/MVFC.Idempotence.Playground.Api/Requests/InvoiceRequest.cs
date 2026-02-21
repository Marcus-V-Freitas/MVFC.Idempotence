namespace MVFC.Idempotence.Playground.Api.Requests;

public sealed record InvoiceRequest(
    string ExternalId, 
    decimal Value);