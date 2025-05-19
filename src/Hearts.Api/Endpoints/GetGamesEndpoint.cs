using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Hearts.Api.Queries;
using Hearts.Contracts.Api;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Hearts.Api.Endpoints;

public static class GetGamesEndpoint
{
    [Produces(typeof(GameResponse))]
    [WolverineGet("/api/games")]
    public static async Task<Microsoft.AspNetCore.Http.IResult> Get([FromServices] IQuerySession querySession)
    {
        Result<GameResponse> response = await GetGamesHandler.Handle(querySession);

        return response.ToMinimalApiResult();
    }
}

public record GameResponse(Game[] Games);
