namespace Hearts.Contracts;

public interface IGameClient
{
    Task CreatePlayer(string name);
    Task PlayerCreated(Player player);

    Task CreateNewGame();
    Task GameUpdated(Game game);

    Task StartGame();
    Task GameStarted(Game game);

    Task RoundStarted(Round round);

    Task PassCards(Guid gameId, PassCard[] passCards);
}
