using Dapr.Actors;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

public interface IGameActor : IActor
{
    Task<List<Player>> Players { get; }
    Task AddPlayer(Player player);
    Task AddBotPlayer();
    Task<Contracts.Round> StartNewRound();

    Task<Game> Map();
}
