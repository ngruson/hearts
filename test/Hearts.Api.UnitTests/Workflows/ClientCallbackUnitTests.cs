using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Hearts.Api.Workflows;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Hearts.Api.UnitTests.Workflows;

public class ClientCallbackUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task game_started(
        [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
        [Substitute, Frozen] GameHub gameHub,
        IGameClient gameClient,
        ClientCallback sut,
        Game game)
    {
        // Arrange

        hubCallerClients.Caller.Returns(gameClient);
        gameHub.Clients.Returns(hubCallerClients);
        
        // Act

        await sut.GameStarted(game);

        // Assert

        await gameHub.Clients.Caller.GameStarted(game);
    }

    [Theory, AutoNSubstituteData]
    internal async Task game_updated(
        [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
        [Substitute, Frozen] GameHub gameHub,
        IGameClient gameClient,
        ClientCallback sut,
        Game game)
    {
        // Arrange

        hubCallerClients.Caller.Returns(gameClient);
        gameHub.Clients.Returns(hubCallerClients);

        // Act

        await sut.GameUpdated(game);

        // Assert

        await gameHub.Clients.Caller.GameUpdated(game);
    }

    [Theory, AutoNSubstituteData]
    internal async Task round_started(
        [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
        [Substitute, Frozen] GameHub gameHub,
        IGameClient gameClient,
        ClientCallback sut,
        Round round)
    {
        // Arrange

        hubCallerClients.Caller.Returns(gameClient);
        gameHub.Clients.Returns(hubCallerClients);

        // Act

        await sut.RoundStarted(round);

        // Assert

        await gameHub.Clients.Caller.RoundStarted(round);
    }
}
