using Dapr.Actors.Runtime;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

internal class GameActor(ActorHost host) : Actor(host), IGameActor
{
    private readonly List<Player> players = [];
    private readonly List<Round> rounds = [];

    public Task<List<Player>> Players => Task.FromResult(this.players);

    public Task AddPlayer(Player player)
    {
        this.players.Add(player);
        return Task.CompletedTask;
    }

    public Task AddBotPlayer()
    {
        //Thread.Sleep(1000);

        Guid playerId = Guid.CreateVersion7();

        string playerName;
        do
        {
            playerName = RandomNameGenerator.GenerateRandomName();
        } while (this.players.Any(p => p.PlayerName == playerName));

        Player player = new(playerId, playerName);
        this.players.Add(player);

        return Task.CompletedTask;
    }

    public Task<Contracts.Round> StartNewRound()
    {
        Round round = Round.Create([.. this.players]);
        this.rounds.Add(round);

        return Task.FromResult(round.Map());
    }

    public Task<Game> Map()
    {
        Game game = new(
            Guid.Parse(this.Id.GetId()),
            [.. this.players]);

        return Task.FromResult(game);
    }
}
