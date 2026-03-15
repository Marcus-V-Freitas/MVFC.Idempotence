namespace MVFC.Idempotence.Models;

internal sealed class RawJsonResult(byte[] jsonBytes, int statusCode = 200) : IResult
{
    private readonly byte[] _jsonBytes = jsonBytes;
    public int StatusCode => statusCode;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.Body.WriteAsync(_jsonBytes, httpContext.RequestAborted).ConfigureAwait(false);
    }
}
