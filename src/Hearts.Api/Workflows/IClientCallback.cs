using Hearts.Contracts;

namespace Hearts.Api.Workflows;

public interface IClientCallback
{
    Task GameStarted(Game game);
    Task GameUpdated(Game game);
    Task RoundStarted(Round round);
}
