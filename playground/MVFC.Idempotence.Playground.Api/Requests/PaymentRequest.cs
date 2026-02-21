namespace MVFC.Idempotence.Playground.Api.Requests;

public sealed record PaymentRequest(
    Guid OrderId, 
    decimal Amount);