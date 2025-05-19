using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Dapr;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

IResourceBuilder<IDaprComponentResource> stateStore = builder.AddDaprStateStore("statestore",
    new DaprComponentOptions
    {
        LocalPath = "./components/statestore.yaml"
    });

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

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Hearts_Api>("backend")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppProtocol = "https"
    })
    .WithReference(pg)
    .WithReference(stateStore)
    .WaitFor(redis);

IResourceBuilder<ProjectResource> frontend = builder.AddProject<Projects.Hearts_BlazorApp>("frontend")
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppProtocol = "https"
    })
    .WithReference(api)
    .WithReference(idSrv)
    .WaitFor(idSrv)
    .WaitFor(api);

idSrv.WithEnvironment("blazorEndpoint", frontend.GetEndpoint("https"));

builder.Build().Run();
