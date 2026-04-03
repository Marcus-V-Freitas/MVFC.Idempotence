namespace MVFC.Idempotence.Playground.Api.Responses;

public sealed record OrderUpdatedResponse(
    Guid OrderId,
    string Status,
    DateTimeOffset UpdatedAt);
