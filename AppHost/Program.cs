using Aspire.Hosting;
using Aspire.Hosting.Dapr;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlazorApp>("frontend");

builder.AddProject<Projects.Api>("backend")
    .WithDaprSidecar(new DaprSidecarOptions
     {
         AppProtocol = "https"
     });

builder.Build().Run();