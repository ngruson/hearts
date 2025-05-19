using Hearts.Api;
using Oakton;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder
    .ConfigureServices()
    .ConfigurePipeline();

await app.RunOaktonCommands(args);
