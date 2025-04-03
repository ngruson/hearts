using Hearts.Contracts.Events;

namespace Hearts.Contracts;

public interface IGameClient
{
    Task CreatePlayer(string name);
    Task PlayerCreated(Player player);

    Task CreateNewGame();
    Task GameUpdated(Game game);
    Task InvalidCardPlayed(InvalidCardPlayedEvent invalidCardPlayedEvent);

    //Task StartGame();
    //Task GameStarted(Game game);

    //Task RoundUpdated(Round round);

    Task PassCards(Guid gameId, PassCard[] passCards);
    Task PlayCard(Guid gameId, Guid playerId, Card card);
    Task TrickCompleted(Game game);
}
