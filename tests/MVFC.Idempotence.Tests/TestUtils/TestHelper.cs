namespace MVFC.Idempotence.Tests.TestUtils;

public static class TestHelper
{
    public static HttpContext CreateHttpContext(IServiceProvider? services = null)
    {
        var context = new DefaultHttpContext();
        if (services != null)
        {
            context.RequestServices = services;
        }
        return context;
    }

    public static IServiceProvider CreateServiceProvider(params (Type ServiceType, object Implementation)[] services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var collection = new ServiceCollection();
        collection.AddLogging();
        foreach (var (serviceType, implementation) in services)
        {
            collection.AddSingleton(serviceType, implementation);
        }
        return collection.BuildServiceProvider();
    }

    public static EndpointFilterFactoryContext CreateFactoryContext(MethodInfo methodInfo) =>
        new() { MethodInfo = methodInfo };
}
