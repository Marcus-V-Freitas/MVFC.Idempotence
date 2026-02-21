namespace MVFC.Idempotence.Tests.TestUtils;

internal static class MockEntities
{
    private const string GlobalHeader = "X-Idempotency-Key";
    private const string PaymentHeader = "X-Request-Id";

    internal static string NewKey() => $"test-{Guid.NewGuid()}";

    internal static Dictionary<string, string> Global(string? value = null) =>
        new()
        {
            { GlobalHeader, value ?? NewKey() }
        };

    internal static Dictionary<string, string> Payment() =>
        new()
        {
            { PaymentHeader, NewKey() }
        };

    internal static Dictionary<string, string> Empty() => [];
}