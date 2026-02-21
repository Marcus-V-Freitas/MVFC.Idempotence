namespace MVFC.Idempotence.Playground.Api.Requests;

public sealed record CreateOrderRequest(
    string ProductId, 
    int Quantity);