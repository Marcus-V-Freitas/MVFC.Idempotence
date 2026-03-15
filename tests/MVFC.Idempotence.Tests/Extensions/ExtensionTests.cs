namespace MVFC.Idempotence.Tests.Extensions;

public sealed class ExtensionTests
{
    private readonly IdempotencyConfig _sut = new();

    [Fact]
    public void AddIdempotencyMemory_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddIdempotencyMemory();

        // Assert
        var provider = services.BuildServiceProvider();
        provider.GetService<IdempotencyConfig>().Should().NotBeNull();
        provider.GetService<IIdempotencyService>().Should().NotBeNull();
    }

    [Fact]
    public void AddIdempotencyRedis_ShouldRegisterRedisAndServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddIdempotencyRedis("localhost:6379");

        // Assert
        var provider = services.BuildServiceProvider();
        provider.GetService<IdempotencyConfig>().Should().NotBeNull();
        provider.GetService<IIdempotencyService>().Should().NotBeNull();
        provider.GetService<IDistributedCache>().Should().NotBeNull();
    }

    [Fact]
    public async Task WithIdempotency_HeaderExtension_CoversLambdas()
    {
        // Arrange
        var builder = Substitute.For<IEndpointConventionBuilder>();
        Action<EndpointBuilder>? captured = null;
        builder.When(x => x.Add(Arg.Any<Action<EndpointBuilder>>())).Do(x => captured = x.Arg<Action<EndpointBuilder>>());

        // Act
        builder.WithIdempotency();
        var endpointBuilder = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse("/"), 0);

        captured!(endpointBuilder);

        var filterFactory = endpointBuilder.FilterFactories[0];
        var factoryContext = TestHelper.CreateFactoryContext(typeof(ExtensionTests).GetMethod(nameof(WithIdempotency_HeaderExtension_CoversLambdas))!);

        var nextCalled = false;
        ValueTask<object?> Next(EndpointFilterInvocationContext _)
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        }

        var filterDelegate = filterFactory.Invoke(factoryContext, Next);
        var services = TestHelper.CreateServiceProvider(
            (typeof(IdempotencyConfig), _sut),
            (typeof(IIdempotencyService), Substitute.For<IIdempotencyService>()));

        var httpContext = TestHelper.CreateHttpContext(services);
        httpContext.Request.Method = "POST";
        httpContext.Request.Headers["Idempotency-Key"] = "val";

        var invocationContext = new DefaultEndpointFilterInvocationContext(httpContext);

        // Act
        await filterDelegate.Invoke(invocationContext);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WithIdempotency_RouteExtension_CoversLambdas()
    {
        // Arrange
        var builder = Substitute.For<IEndpointConventionBuilder>();
        Action<EndpointBuilder>? captured = null;
        builder.When(x => x.Add(Arg.Any<Action<EndpointBuilder>>())).Do(x => captured = x.Arg<Action<EndpointBuilder>>());

        // Act
        builder.WithIdempotency("id");
        var endpointBuilder = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse("/"), 0);
        captured!(endpointBuilder);

        var filterFactory = endpointBuilder.FilterFactories[0];
        var factoryContext = TestHelper.CreateFactoryContext(typeof(ExtensionTests).GetMethod(nameof(WithIdempotency_RouteExtension_CoversLambdas))!);

        static ValueTask<object?> Next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(Results.Ok());

        var filterDelegate = filterFactory.Invoke(factoryContext, Next);
        var services = TestHelper.CreateServiceProvider(
            (typeof(IdempotencyConfig), _sut),
            (typeof(IIdempotencyService), Substitute.For<IIdempotencyService>()));

        var httpContext = TestHelper.CreateHttpContext(services);
        httpContext.Request.Method = "POST";
        httpContext.Request.RouteValues["id"] = "route-val";

        var invocationContext = new DefaultEndpointFilterInvocationContext(httpContext);

        // Act & Assert - Hits the NotNull branch
        var result1 = await filterDelegate.Invoke(invocationContext);
        result1!.GetType().Name.Should().Contain("Ok", Exactly.Once());

        // Act & Assert - Hits the Null branch (missing route value)
        httpContext.Request.RouteValues.Clear();
        var result2 = await filterDelegate.Invoke(invocationContext);
        result2!.GetType().Name.Should().Contain("BadRequest", Exactly.Once());
    }

    [Fact]
    public void WithIdempotency_ResolverExtension_ShouldAddFilter()
    {
        // Arrange
        var builder = Substitute.For<IEndpointConventionBuilder>();
        static string? KeyResolver(HttpContext ctx) => "custom";

        // Act
        builder.WithIdempotency(KeyResolver);

        // Assert
        builder.Received(1).Add(Arg.Any<Action<EndpointBuilder>>());
    }
}
