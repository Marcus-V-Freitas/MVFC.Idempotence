namespace MVFC.Idempotence.Tests.Models;

internal sealed class CustomResult(int statusCode) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}
