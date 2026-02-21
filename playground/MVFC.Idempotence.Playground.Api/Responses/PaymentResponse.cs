namespace MVFC.Idempotence.Playground.Api.Responses;

public sealed record PaymentResponse(
    Guid TransactionId, 
    Guid OrderId, 
    decimal Amount, 
    DateTime ProcessedAt);