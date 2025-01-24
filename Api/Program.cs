using Api;
using Api.Actors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<GameActor>();
    options.UseJsonSerialization = true;
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<GameHub>("/gameHub");

app.Run();