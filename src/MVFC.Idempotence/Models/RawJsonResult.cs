namespace MVFC.Idempotence.Models;

internal sealed class RawJsonResult(byte[] jsonBytes, int statusCode = 200) : IResult
{
    private readonly byte[] _jsonBytes = jsonBytes;
    private readonly int _statusCode = statusCode;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.Body.WriteAsync(_jsonBytes, httpContext.RequestAborted);
    }
}