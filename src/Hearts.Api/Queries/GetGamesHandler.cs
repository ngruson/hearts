using Ardalis.Result;
using Hearts.Api.Endpoints;
using Hearts.Contracts;
using Marten;

namespace Hearts.Api.Queries;

public static class GetGamesHandler
{
    public static async Task<Result<GameResponse>> Handle(IQuerySession querySession, bool showUpcomingOnly = false)
    {
        IQueryable<Eventing.Entities.Game> queryable = querySession.Query<Eventing.Entities.Game>();
        if (showUpcomingOnly)
        {
            queryable = queryable.Where(game => game.State == GameState.Registering);
        }

        IReadOnlyList<Eventing.Entities.Game> games = await queryable.ToListAsync();

        if (!games.Any())
        {
            return Result.NotFound("No registering games found.");
        }

        return new GameResponse(
            [.. games.Select(game => new Contracts.Api.Game(
                game.Id,
                "Hearts",
                game.State,
                game.Players.Length
            ))]
        );
    }
}
