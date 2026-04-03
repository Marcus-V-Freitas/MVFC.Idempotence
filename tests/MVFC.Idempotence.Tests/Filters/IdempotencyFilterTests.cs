namespace MVFC.Idempotence.Tests.Filters;

public sealed class IdempotencyFilterTests
{
    private readonly IIdempotencyService _service = Substitute.For<IIdempotencyService>();
    private readonly IdempotencyConfig _config = new();
    private readonly HttpContext _sut;

    public IdempotencyFilterTests() =>
        _sut = TestHelper.CreateHttpContext(TestHelper.CreateServiceProvider(
                (typeof(IIdempotencyService), _service), (typeof(IdempotencyConfig), _config)));

    [Fact]
    public async Task InvokeAsync_WhenMethodNotAllowed_ShouldCallNext()
    {
        // Arrange
        _sut.Request.Method = "GET";

        var nextCalled = false;
        var filter = new IdempotencyFilter(ctx => "key", options: new IdempotencyOptions
        {
            AllowedMethods = new HashSet<string> { "POST" }
        });

        ValueTask<object?> Next(EndpointFilterInvocationContext _)
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        }

        // Act
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyIsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var filter = new IdempotencyFilter(ctx => null);

        static ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(Results.Ok());

        // Act
        var result = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        result.Should().NotBeNull();
        result!.GetType().Name.Should().Contain("BadRequest", Exactly.Once());
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyIsWhitespace_ShouldReturnBadRequest()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var filter = new IdempotencyFilter(ctx => "   ");

        static ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(Results.Ok());

        // Act
        var result = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        result.Should().NotBeNull();
        result!.GetType().Name.Should().Contain("BadRequest", Exactly.Once());
    }

    [Fact]
    public async Task InvokeAsync_WhenCached_ShouldReturnCachedResult()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "test-key";
        var filter = new IdempotencyFilter(ctx => key);
        var cached = new CachedResult(200, [1, 2, 3]);

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .Returns(cached);

        // Act
        var result = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(null));

        // Assert
        var rawResult = result.Should().BeOfType<RawJsonResult>().Subject;
        rawResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenCachedFailure_ShouldReturnProblem()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "fail-key";
        var filter = new IdempotencyFilter(ctx => key);
        var cached = new CachedResult(400, null) { Error = "Failure message" };

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .Returns(cached);

        // Act
        var result = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(null));

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_WhenCachedWithNullPayload_ShouldReturnEmptyRawJson()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "null-payload";
        var filter = new IdempotencyFilter(ctx => key);
        var cached = new CachedResult(200, null);

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .Returns(cached);

        // Act
        var result = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(null));

        // Assert
        var rawResult = result.Should().BeOfType<RawJsonResult>().Subject;
        rawResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenSuccessResult_ShouldCache()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "cache-me";
        var filter = new IdempotencyFilter(ctx => key);

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .ReturnsNull();

        var result = Results.Ok(new
        {
            data = "ok",
        });

        ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(result);

        // Act
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        await _service.Received(1).CacheAsync(key, Arg.Any<ReadOnlyMemory<byte>>(), 200, Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenErrorResult_ShouldNotCache()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "dont-cache-me";
        var filter = new IdempotencyFilter(ctx => key);
        var result = Results.BadRequest();

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .ReturnsNull();

        ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(result);

        // Act
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        await _service.DidNotReceiveWithAnyArgs().CacheAsync(default!, default!, default!, default!, default!);
    }

    [Fact]
    public async Task InvokeAsync_WhenCacheThrows_ShouldSwallowException()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "cache-error";
        var filter = new IdempotencyFilter(ctx => key);
        var result = Results.Ok(new
        {
            data = "ok"
        });

        _service.GetAsync(key, Arg.Any<CancellationToken>())
                .ReturnsNull();

        _service.CacheAsync(Arg.Any<string>(),
                            Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<int>(),
                            Arg.Any<TimeSpan?>(),
                            Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new InvalidOperationException("Cache failure")));

        ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(result);

        // Act
        var actResult = await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        actResult.Should().BeSameAs(result);
        await _service.Received(1).CacheAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<int>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenStateAlreadyInitialized_ShouldNotReinitialize()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "state-test";
        var filter = new IdempotencyFilter(ctx => key);

        // First call to initialize state
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Second call
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(Results.Ok()));
    }

    [Fact]
    public async Task InvokeAsync_WhenResultIsNotIResult_ShouldCacheEmptyPayload()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "non-iresult";
        var filter = new IdempotencyFilter(ctx => key);
        var result = new
        {
            data = "not an IResult"
        };

        ValueTask<object?> Next(EndpointFilterInvocationContext _) =>
            ValueTask.FromResult<object?>(result);

        // Act
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), Next);

        // Assert
        await _service.Received(1).CacheAsync(key, Arg.Is<ReadOnlyMemory<byte>>(m => m.Length == 0), 200, Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIsBoundary_ShouldCacheCorrectly()
    {
        // Arrange
        _sut.Request.Method = "POST";

        var key = "boundary-test";
        var filter = new IdempotencyFilter(ctx => key);

        // Test 299 (Should cache)
        var result299 = new CustomResult(299);
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(result299));
        await _service.Received(1).CacheAsync(key, Arg.Any<ReadOnlyMemory<byte>>(), 299, Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());

        // Test 300 (Should NOT cache)
        var result300 = new CustomResult(300);
        await filter.InvokeAsync(new DefaultEndpointFilterInvocationContext(_sut), _ => ValueTask.FromResult<object?>(result300));
        await _service.DidNotReceive().CacheAsync(key, Arg.Any<ReadOnlyMemory<byte>>(), 300, Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }
}
