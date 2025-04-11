using Hearts.Api;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;
using Hearts.ServiceDefaults;
using Microsoft.AspNetCore.ResponseCompression;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<GameActor>();
    options.UseJsonSerialization = true;
});

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});
builder.Services.AddSingleton<GameHub>();
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
