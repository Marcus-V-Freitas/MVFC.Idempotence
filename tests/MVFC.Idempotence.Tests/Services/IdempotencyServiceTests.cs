namespace MVFC.Idempotence.Tests.Services;

public sealed class IdempotencyServiceTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();
    private readonly IdempotencyConfig _config = new();
    private readonly IdempotencyService _sut;

    public IdempotencyServiceTests() =>
        _sut = new(_cache, _config);

    [Fact]
    public void Constructor_WhenCacheIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => new IdempotencyService(null!, _config);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("cache");
    }

    [Fact]
    public void Constructor_WhenConfigIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => new IdempotencyService(_cache, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreviousFailure_ThrowsIdempotencyException()
    {
        // Arrange
        var key = "fail-key";
        var expected = new CachedModel<string>(400, null, "Error");

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(expected));

        // Act & Assert
        var ex = await _sut.Invoking(s => s.ExecuteAsync(key, ct => Task.FromResult("ok"), ct: TestContext.Current.CancellationToken))
                           .Should()
                           .ThrowAsync<IdempotencyException>();
        
        ex.Which.Message.Should().Be("Error");
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreviousFailureMissingError_ThrowsDefaultException()
    {
        // Arrange
        var key = "fail-no-error";
        var expected = new CachedModel<string>(400, null, null);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(expected));
        
        // Act & Assert
        var ex = await _sut.Invoking(s => s.ExecuteAsync(key, ct => Task.FromResult("ok"), ct: TestContext.Current.CancellationToken))
                           .Should()
                           .ThrowAsync<IdempotencyException>();
        
        ex.Which.Message.Should().Be("Operação anterior falhou.");
    }

    [Fact]
    public async Task ExecuteAsync_WithByteArray_ReturnsCorrectPayload()
    {
        // Arrange
        var key = "bytes";
        var expectedPayload = new byte[] { 1, 2, 3 };
        var model = new CachedModel<byte[]>(200, expectedPayload);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<byte[]>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(model));

        // Act
        var result = await _sut.ExecuteAsync(key, ct => Task.FromResult(new byte[] { 4, 5 }), ct: TestContext.Current.CancellationToken);

        // Assert
        result.Should().Equal(expectedPayload);
    }

    [Fact]
    public async Task GetAsync_WhenCached_ReturnsResult()
    {
        // Arrange
        var key = "get-key";
        var expected = new CachedResult(200, [1]);
        
        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedResult?>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<CachedResult?>(expected));

        // Act
        var result = await _sut.GetAsync(key, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task CacheAsync_ShouldCallSet()
    {
        // Arrange
        var key = "cache-async";
        var payload = "\n"u8.ToArray();

        // Act
        await _sut.CacheAsync(key, payload, 201, ct: TestContext.Current.CancellationToken);

        // Assert
        await _cache.Received(1).SetAsync(Arg.Any<string>(), Arg.Is<CachedResult>(r => r.Status == 201), Arg.Any<HybridCacheEntryOptions>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTTL_UsesShortest()
    {
        // Arrange
        _config.Ttl = TimeSpan.FromMinutes(10);

        var customTtl = TimeSpan.FromMinutes(5);
        var key = "ttl-test";

        // Mock return to avoid NRE and verify factory call
        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<Func<CancellationToken, ValueTask<CachedModel<string>>>>()(x.Arg<CancellationToken>()));

        // Act
        var result = await _sut.ExecuteAsync(key, ct => Task.FromResult("ok"), customTtl, ct: TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be("ok");
        await _cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == customTtl),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomTtlIsLonger_UsesProvided()
    {
        // Arrange
        _config.Ttl = TimeSpan.FromMinutes(10);

        var longerTtl = TimeSpan.FromMinutes(15);
        var key = "ttl-test-long";

        // Mock return
        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(new CachedModel<string>(200, "ok")));

        // Act
        await _sut.ExecuteAsync(key, ct => Task.FromResult("ok"), longerTtl, ct: TestContext.Current.CancellationToken);

        // Assert
        await _cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == longerTtl),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoTtlProvided_UsesDefault()
    {
        // Arrange
        _config.Ttl = TimeSpan.FromMinutes(10);
        var key = "ttl-test-none";

        // Mock return
        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(new CachedModel<string>(200, "ok")));

        // Act
        await _sut.ExecuteAsync(key, ct => Task.FromResult("ok"), ct: TestContext.Current.CancellationToken);

        // Assert
        await _cache.Received(1).GetOrCreateAsync(
             Arg.Any<string>(),
             Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
             Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == _config.Ttl),
             Arg.Any<IEnumerable<string>>(),
             Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenShortTtl_UsesTtlForLocalCache()
    {
        // Arrange
        var shortTtl = TimeSpan.FromMinutes(1);
        var key = "short-ttl";

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
                .Returns(ValueTask.FromResult(new CachedModel<string>(200, "ok")));

        // Act
        await _sut.ExecuteAsync(key, ct => Task.FromResult("ok"), shortTtl, ct: TestContext.Current.CancellationToken);

        // Assert
        await _cache.Received(1).GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<CachedModel<string>>>>(),
            Arg.Is<HybridCacheEntryOptions>(o => o.LocalCacheExpiration == shortTtl),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheAsync_WhenShortTtl_UsesTtlForLocalCache()
    {
        // Arrange
        var shortTtl = TimeSpan.FromMinutes(1);
        var key = "short-ttl-cache";

        // Act
        await _sut.CacheAsync(key, new byte[] { 1 }, 200, shortTtl, ct: TestContext.Current.CancellationToken);

        // Assert
        await _cache.Received(1).SetAsync(
            Arg.Any<string>(),
            Arg.Any<CachedResult>(),
            Arg.Is<HybridCacheEntryOptions>(o => o.LocalCacheExpiration == shortTtl),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }
}
