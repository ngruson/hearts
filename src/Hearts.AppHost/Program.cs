using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Dapr;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

IResourceBuilder<IDaprComponentResource> stateStore = builder.AddDaprStateStore("statestore",
    new DaprComponentOptions
    {
        LocalPath = "./components/statestore.yaml"
    });

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Hearts_Api>("backend")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppProtocol = "https"
    })
    .WithReference(stateStore)
    .WaitFor(redis);

builder.AddProject<Projects.Hearts_BlazorApp>("frontend")
    .WithReference(api);

builder.Build().Run();
