using Hearts.BlazorApp.Services.SignalR;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hearts.BlazorApp.Services.SignalR;

public class SignalRService
{
    private readonly HubConnection hubConnection;
    private bool isStarted;

    public Player? Player { get; private set; }

    public event EventHandler<Player>? PlayerCreated;
    public event EventHandler<Game>? GameUpdated;
    public event EventHandler<Round>? RoundStarted;

    public SignalRService(HubConnection hubConnection)
    {
        this.hubConnection = hubConnection;

        this.hubConnection.On<Player>(nameof(IGameClient.PlayerCreated), player =>
        {
            this.Player = player;
            this.OnPlayerCreated(player);
        });

        this.hubConnection.On<Game>(nameof(IGameClient.GameUpdated), game =>
        {            
            this.OnGameUpdated(game);
        });

        this.hubConnection.On<Round>(nameof(IGameClient.RoundStarted), round =>
        {
            this.OnRoundStarted(round);
        });
    }

    private void OnPlayerCreated(Player player)
    {
        PlayerCreated?.Invoke(this, player);
    }    

    public async Task CreatePlayer(string name)
    {
        await this.hubConnection.SendAsync(nameof(IGameClient.CreatePlayer), name);
    }

    private void OnGameUpdated(Game game)
    {
        GameUpdated?.Invoke(this, game);
    }

    private void OnRoundStarted(Round round)
    {
        RoundStarted?.Invoke(this, round);
    }

    public async Task CreateNewGame(Player player)
    {
        await this.hubConnection.SendAsync(nameof(IGameClient.CreateNewGame), player);
    }

    public async Task StartAsync()
    {
        if (!this.isStarted)
        {
            await this.hubConnection.StartAsync();
            this.isStarted = true;
        }
    }

    public async Task StopAsync()
    {
        if (this.isStarted)
        {
            await this.hubConnection.StopAsync();
            this.isStarted = false;
        }
    }
}


