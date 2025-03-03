using Dapr.Actors.Runtime;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

internal class GameActor(ActorHost host) : Actor(host), IGameActor
{
    private readonly List<Player> players = [];
    private readonly List<Round> rounds = [];
    private PassingDirection PassingDirection = PassingDirection.None;

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

        BotPlayer player = new(playerId, playerName);
        this.players.Add(player);

        return Task.CompletedTask;
    }

    public Task<Contracts.Round> StartNewRound()
    {
        Round round = Round.Create([.. this.players]);
        this.rounds.Add(round);
        
        this.ChangePassingDirection();

        return Task.FromResult(round.Map());
    }

    private void ChangePassingDirection()
    {
        switch (this.PassingDirection)
        {
            case PassingDirection.Left:
                this.PassingDirection = PassingDirection.Right;
                break;
            case PassingDirection.Right:
                this.PassingDirection = PassingDirection.Across;
                break;
            case PassingDirection.Across:
                this.PassingDirection = PassingDirection.None;
                break;
            case PassingDirection.None:
                this.PassingDirection = PassingDirection.Left;
                break;
        }
    }

    public Task<Game> Map(string workflowInstanceId)
    {
        Game game = new(
            Guid.Parse(this.Id.GetId()),
            workflowInstanceId,
            [.. this.players],
            this.PassingDirection);

        return Task.FromResult(game);
    }

    public Task PassCards(PassCard[] passCards)
    {
        Round round = this.rounds[0];

        foreach (PassCard passCard in passCards)
        {
            RoundPlayer fromPlayer = round.Players.Single(p => p.Player.Id == passCard.FromPlayerId);
            RoundPlayer toPlayer = round.Players.Single(p => p.Player.Id == passCard.ToPlayerId);
            fromPlayer.Cards = [.. fromPlayer.Cards.Where(_ => _.Suit != passCard.Card.Suit && _.Rank != passCard.Card.Rank)];
            toPlayer.Cards = [.. toPlayer.Cards, passCard.Card];
        }

        return Task.CompletedTask;
    }
}
