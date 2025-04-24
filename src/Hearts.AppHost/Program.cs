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

IResourceBuilder<PostgresServerResource> pg = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("pg-data")
    .WithPgAdmin(config =>
    {
        config.WithImageTag("latest");
        config.WithLifetime(ContainerLifetime.Persistent);
    });

IResourceBuilder<ProjectResource> idSrv = builder.AddProject<Projects.Hearts_IdentityServer>("identity")
    .WithReference(pg)
    .WaitFor(pg);

IResourceBuilder<ProjectResource> frontend = builder.AddProject<Projects.Hearts_BlazorApp>("frontend")
    .WithReference(api)
    .WithReference(idSrv)
    .WaitFor(idSrv)
    .WithEnvironment("ASPNETCORE_URLS", "https://localhost:7220");

idSrv.WithEnvironment("blazorEndpoint", "https://localhost:7220");

builder.Build().Run();
