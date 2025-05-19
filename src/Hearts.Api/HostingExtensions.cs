using Hearts.Api.Actors;
using Hearts.Api.Eventing.Entities;
using Hearts.Api.Eventing.Projections;
using Hearts.Api.OpenTelemetry;
using Hearts.Api.Services;
using Hearts.ServiceDefaults;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.ResponseCompression;
using Scalar.AspNetCore;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

namespace Hearts.Api;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
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

        builder.Services.AddMarten(options =>
        {
            options.Connection(builder.Configuration.GetConnectionString("postgres")!);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = "hearts";
            options.UseSystemTextJsonForSerialization();
            options.Projections.Add<GameProjection>(ProjectionLifecycle.Inline);
        })
        .UseLightweightSessions()
        .IntegrateWithWolverine();

        builder.Services.AddHostedService<GameMonitorService>();
        builder.Services.AddWolverineHttp();

        builder.UseWolverine(options =>
        {
            options.Policies.AutoApplyTransactions();
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        app.MapHub<GameHub>("gameHub");

        app.MapActorsHandlers();

        app.MapWolverineEndpoints();

        app.MapGet("games", async (IQuerySession querySession) =>
        {
            IReadOnlyList<Game> games = await querySession.Query<Game>().ToListAsync();
            return Results.Ok(games);
        });

        app.MapGet("games/{gameId:guid}", async (IQuerySession querySession, Guid gameId) =>
        {
            GameProjection? game = await querySession.LoadAsync<GameProjection>(gameId.ToString());
            return game is not null ? Results.Ok(game) : Results.NotFound();
        });


        return app;
    }
}
