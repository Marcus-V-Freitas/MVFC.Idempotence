var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration.GetConnectionString("idempotency-cache")!;

builder.Services.AddIdempotencyRedis(redisConnection, cfg =>
{
    cfg.Ttl = TimeSpan.FromHours(24);
    cfg.HeaderName = "X-Idempotency-Key";
    cfg.AllowedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };
});

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();