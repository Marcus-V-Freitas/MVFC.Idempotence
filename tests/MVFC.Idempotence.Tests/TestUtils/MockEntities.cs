namespace MVFC.Idempotence.Tests.TestUtils;

internal static class MockEntities
{
    private const string GLOBAL_HEADER = "X-Idempotency-Key";
    private const string PAYMENT_HEADER = "X-Request-Id";

    internal static string NewKey() => $"test-{Guid.NewGuid()}";

    internal static Dictionary<string, string> Global(string? value = null) =>
        new()
        {
            { GLOBAL_HEADER, value ?? NewKey() }
        };

    internal static Dictionary<string, string> Payment() =>
        new()
        {
            { PAYMENT_HEADER, NewKey() }
        };

    internal static Dictionary<string, string> Empty() => [];
}
