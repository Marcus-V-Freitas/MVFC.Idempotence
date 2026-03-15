namespace MVFC.Idempotence.Tests.Fixture;

public sealed class AspireFixture : IAsyncLifetime
{
    private ProjectAppHost _appHost = null!;
    private HttpClient _http = null!;

    internal IApiService Api = null!;

    public async ValueTask InitializeAsync()
    {
        _appHost = new ProjectAppHost();
        await _appHost.StartAsync().ConfigureAwait(false);

        _http = _appHost.CreateHttpClient("api");
        Api = RestService.For<IApiService>(_http);
    }

    public async ValueTask DisposeAsync()
    {
        await _appHost.DisposeAsync().ConfigureAwait(false);
        _http.Dispose();
    }
}
