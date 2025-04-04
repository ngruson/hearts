using Ardalis.Result;
using Dapr.Actors;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

public interface IGameActor : IActor
{    
    Task AddPlayer(Player player);
    Task AddBotPlayer();
    Task ChangePlayerTurn();
    Task<Game> Map();
    Task PlayBots();
    Task PassCards(PassCard[] passCards); 
    Task PlayCard(Guid playerId, Card card);
    Task StartRound();
    Task StartTrick();
    Task<Result> ValidateCard(Card card);
}
