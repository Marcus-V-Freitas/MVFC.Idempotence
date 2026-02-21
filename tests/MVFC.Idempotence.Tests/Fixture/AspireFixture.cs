namespace MVFC.Idempotence.Tests.Fixture;

public sealed class AspireFixture : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private HttpClient _http = null!;

    internal IApiService Api = null!;

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MVFC_Idempotence_Playground_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        _http = _app.CreateHttpClient("api");
        Api = RestService.For<IApiService>(_http);
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        _http.Dispose();
    }
}