using Hearts.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder
    .ConfigureServices()
    .ConfigurePipeline();

app.Run();
