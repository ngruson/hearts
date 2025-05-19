using Hearts.BlazorApp.Components;
using Hearts.BlazorApp.Services;
using Hearts.BlazorApp.Services.LocalStorage;
using Hearts.BlazorApp.Services.SignalR;
using Hearts.ServiceDefaults;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hearts.BlazorApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
        builder.AddServiceDefaults();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddTransient(sp => new HubConnectionBuilder()
           .WithUrl(new Uri($"{builder.Configuration["services:backend:https:0"]}/gameHub"))
           .WithAutomaticReconnect()
           .Build());

        builder.Services.AddSingleton(sp =>
        {
            HubConnection hubConnection = sp.GetRequiredService<HubConnection>();
            ILogger<HubConnection> logger = sp.GetRequiredService<ILogger<HubConnection>>();

            hubConnection.Reconnecting += (exception) =>
            {
                if (exception != null)
                {
                    logger.LogError(exception, "An error occurred while reconnecting.");
                }

                logger.LogInformation("Reconnecting...");
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += (connectionId) =>
            {
                logger.LogInformation("Reconnected to server with connection ID: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            SignalRService signalRService = new(hubConnection);
            //Task.Run(async () => await signalRService.StartAsync());

            return signalRService;
        });

        builder.Services.AddScoped<DarkModeService>();
        builder.Services.AddScoped<LocalStorageService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = builder.Configuration["services:identity:https:0"];
                options.ClientId = builder.Configuration["oidc:clientId"];
                options.ClientSecret = builder.Configuration["oidc:clientSecret"];
                options.MapInboundClaims = false;
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.Scope.Add("offline_access");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
            });

        builder.Services.AddDaprClient();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAntiforgery();
        app.UseAuthorization();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
