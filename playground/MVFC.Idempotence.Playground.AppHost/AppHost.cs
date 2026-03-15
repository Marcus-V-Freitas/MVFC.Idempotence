var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("idempotency-cache");

builder.AddProject<Projects.MVFC_Idempotence_Playground_Api>("api")
       .WithReference(redis)
       .WaitFor(redis);

await builder.Build().RunAsync().ConfigureAwait(false);
