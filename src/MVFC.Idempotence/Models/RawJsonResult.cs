namespace MVFC.Idempotence.Models;

internal sealed class RawJsonResult(string json, int statusCode = 200) : IResult
{
    private readonly string _json = json;
    private readonly int _statusCode = statusCode;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = "application/json";
        return httpContext.Response.WriteAsync(_json, httpContext.RequestAborted);
    }
}