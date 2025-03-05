using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class ClientCallback(GameHub hub) : IClientCallback
{
    public virtual async Task GameStarted(Game game)
    {
        await hub.Clients.Caller.GameStarted(game);
    }    

    public virtual async Task GameUpdated(Game game)
    {
        await hub.Clients.Caller.GameUpdated(game);
    }

    public virtual async Task RoundStarted(Round round)
    {
        await hub.Clients.Caller.RoundStarted(round);
    }
}
