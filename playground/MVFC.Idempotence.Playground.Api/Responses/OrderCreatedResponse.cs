namespace MVFC.Idempotence.Playground.Api.Responses;

public sealed record OrderCreatedResponse(
    Guid OrderId, 
    string ProductId, 
    int Quantity, 
    DateTime CreatedAt);