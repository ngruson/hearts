using Hearts.BlazorApp.Services.SignalR;
using Hearts.Contracts;
using Hearts.Contracts.Events;
using Microsoft.AspNetCore.SignalR.Client;

namespace Hearts.BlazorApp.Services.SignalR;

public class SignalRService
{
    private readonly HubConnection hubConnection;

    public Player? Player { get; private set; }

    public event EventHandler<Player>? PlayerCreated;
    public event EventHandler<Game>? GameUpdated;
    public event EventHandler<InvalidCardPlayedEvent>? InvalidCardPlayed;

    public SignalRService(HubConnection hubConnection)
    {
        this.hubConnection = hubConnection;

        this.hubConnection.On<Player>(nameof(IGameClient.PlayerCreated), player =>
        {
            this.Player = player;
            this.OnPlayerCreated(player);
        });

        this.hubConnection.On<Game>(nameof(IGameClient.GameUpdated), this.OnGameUpdated);
        this.hubConnection.On<InvalidCardPlayedEvent>(nameof(IGameClient.InvalidCardPlayed), this.OnInvalidCardPlayed);
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

    private void OnInvalidCardPlayed(InvalidCardPlayedEvent invalidCardPlayedEvent)
    {
        InvalidCardPlayed?.Invoke(this, invalidCardPlayedEvent);
    }

    public async Task CreateNewGame(Player player)
    {
        await this.hubConnection.SendAsync(nameof(IGameClient.CreateNewGame), player);
    }

    public async Task PassCards(Game game, PassCard[]? passCards)
    {
        await this.hubConnection.SendAsync(nameof(IGameClient.PassCards), game.Id, passCards);
    }

    public async Task PlayCard(Game game, RoundPlayer roundPlayer, Card card)
    {
        await this.hubConnection.SendAsync(nameof(IGameClient.PlayCard), game.Id, roundPlayer.Player.Id, card);
    }

    public async Task StartAsync()
    {
        if (this.hubConnection.State == HubConnectionState.Disconnected)
        {
            await this.hubConnection.StartAsync();
        }
    }

    public async Task StopAsync()
    {
        if (this.hubConnection.State == HubConnectionState.Connected)
        {
            await this.hubConnection.StopAsync();
        }
    }
}
