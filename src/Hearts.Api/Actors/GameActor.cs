using Ardalis.Result;
using Dapr.Actors.Runtime;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

internal class GameActor(ActorHost host) : Actor(host), IGameActor
{
    public Round? CurrentRound => this.Rounds.LastOrDefault();
    public PassingDirection PassingDirection { get; set; } = PassingDirection.None;
    public Player[] Players { get; set; } = [];
    public Round[] Rounds { get; set; } = [];
    public PlayerScore[] Scores => this.CalculateScore();
    public bool IsCompleted => this.Scores.Any(_ => _.Points >= 100)
        && this.Rounds.All(_ => _.IsCompleted)
        && this.Scores.GroupBy(score => score.Points).All(group => group.Count() == 1);

    public async Task AddBotPlayer()
    {
        Guid playerId = Guid.CreateVersion7();

        string playerName;
        do
        {
            playerName = RandomNameGenerator.GenerateRandomName();
        }
        while (this.Players.Any(p => p.PlayerName == playerName));

        Player player = new(playerId, playerName, true);
        await this.AddPlayer(player);
    }

    public Task AddPlayer(Player player)
    {
        this.Players = [.. this.Players, player];
        return Task.CompletedTask;
    }

    public Task ChangePlayerTurn()
    {
        this.CurrentRound?.ChangePlayerTurn();
        return Task.CompletedTask;
    }

    public Task<Game> Map()
    {
        string? message = null;

        if (this.CurrentRound?.SelectingCards == true)
        {
            message = "Select 3 cards to pass to ";
        }

        Game game = new(
            Guid.Parse(this.Id.GetId()),
            [.. this.Players],            
            [.. this.Rounds.Select(_ => _.Map())],
            [.. this.Scores.Select(_ => _.Map())],            
            this.PassingDirection,
            this.IsCompleted,
            message);

        return Task.FromResult(game);
    }

    public Task PassCards(PassCard[] passCards)
    {
        if (this.CurrentRound is not null && this.PassingDirection != PassingDirection.None)
        {
            this.CurrentRound.PassCards(passCards);
        }
        
        return Task.CompletedTask;
    }

    public Task PlayBots()
    {
        this.CurrentRound?.PlayBots();
        
        return Task.CompletedTask;
    }

    public async Task PlayCard(Guid playerId, Card card)
    {
        this.CurrentRound?.PlayCard(playerId, card);
        
        await Task.CompletedTask;
    }

    public Task StartRound()
    {
        this.ChangePassingDirection();

        Round round = Round.Create(
            Guid.Parse(this.Id.GetId()),
            [.. this.Players],
            this.PassingDirection != PassingDirection.None);

        this.Rounds = [.. this.Rounds, round];

        return Task.CompletedTask;
    }

    public Task StartTrick()
    {
        this.CurrentRound?.StartTrick();
        
        return Task.CompletedTask;
    }

    public Task<Result> ValidateCard(Card card)
    {
        if (this.CurrentRound == null)
        {
            return Task.FromResult(Result.Invalid(new ValidationError("Game has not started")));
        }

        Result validationResult = this.CurrentRound.ValidateCard(card);

        return Task.FromResult(validationResult);
    }

    private PlayerScore[] CalculateScore()
    {
        return [.. this.Rounds
            .Where(round => round.Scores != null)
            .SelectMany(round => round.Scores!)
            .GroupBy(score => score.PlayerId)
            .Select(group => new PlayerScore(
                group.Key,
                this.Players.Single(player => player.Id == group.Key).PlayerName,
                group.Sum(score => score.Points)))];
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
}
