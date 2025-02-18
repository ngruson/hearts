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
    .Build());

builder.Services.AddSingleton(sp =>
{
    HubConnection hubConnection = sp.GetRequiredService<HubConnection>();
    SignalRService signalRService = new(hubConnection);
    Task.Run(async () => await signalRService.StartAsync());

    return signalRService;
});

builder.Services.AddScoped<LocalStorageService>();

await builder.Build().RunAsync();
