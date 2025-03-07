using Dapr.Workflow;
using Hearts.Api;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;
using Hearts.Api.Workflows;
using Hearts.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<GameActor>();
    options.UseJsonSerialization = true;
});

builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<GameWorkflow>();

    options.RegisterActivity<AddBotPlayerActivity>();
    options.RegisterActivity<CardsPassedActivity>();
    options.RegisterActivity<CreateNewGameActivity>();
    options.RegisterActivity<NotifyGameUpdatedActivity>();
    options.RegisterActivity<NotifyRoundStartedActivity>();    
    options.RegisterActivity<StartNewRoundActivity>();
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameHub>();
builder.Services.AddSingleton<IClientCallback, ClientCallback>();
builder.Services.AddSingleton<Instrumentation>();

builder.Services.AddCors();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

//app.UseHttpsRedirection();

app.MapHub<GameHub>("gameHub");

app.MapActorsHandlers();

app.Run();
