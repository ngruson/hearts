using Hearts.BlazorApp;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder
    .ConfigureServices()
    .ConfigurePipeline();

await app.RunAsync();
