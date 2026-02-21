namespace MVFC.Idempotence.Models;

internal sealed class BufferedHttpResponse
{
    public HttpContext Context { get; }

    internal BufferedHttpResponse(HttpContext original, MemoryStream buffer)
    {
        Context = new DefaultHttpContext
        {
            RequestServices = original.RequestServices,
            TraceIdentifier = original.TraceIdentifier
        };

        Context.Response.Body = buffer;
        Context.Response.StatusCode = original.Response.StatusCode;
        Context.Response.ContentType = original.Response.ContentType;
    }
}