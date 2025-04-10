using Hearts.BlazorApp;
using Hearts.BlazorApp.Services.LocalStorage;
using Hearts.BlazorApp.Services.SignalR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddTransient(sp => new HubConnectionBuilder()
    .WithUrl(new Uri($"{configuration["backend"]}/gameHub"))
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
    Task.Run(async () => await signalRService.StartAsync());

    return signalRService;
});

builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();
