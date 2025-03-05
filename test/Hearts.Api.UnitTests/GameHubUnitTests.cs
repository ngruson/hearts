using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Hearts.Api.UnitTests;

public class GameHubUnitTests
{
    public class CreatePlayer
    {
        [Theory, AutoNSubstituteData]
        internal async Task create_player(
        [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
        GameHub sut,
        string playerName)
        {
            // Arrange

            // Act

            await sut.CreatePlayer(playerName);

            // Assert

            await hubCallerClients.Caller.Received().PlayerCreated(Arg.Any<Player>());
        }

        [Theory, AutoNSubstituteData]
        internal async Task return_null_when_caller_is_null(
            [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
            GameHub sut,
            string playerName)
        {
            // Arrange

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            hubCallerClients.Caller.Returns((IGameClient)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Act

            await sut.CreatePlayer(playerName);

            // Assert

            Assert.Null(hubCallerClients.Caller);
        }
    }

    public class CreateNewGame
    {
        [Theory, AutoNSubstituteData]
        internal async Task create_new_game(
            [Substitute, Frozen] DaprWorkflowClient daprWorkflowClient,
            GameHub sut,
            Player player)
        {
            // Arrange

            // Act

            await sut.CreateNewGame(player);

            // Assert

            Assert.NotNull(daprWorkflowClient);
        }
    }

    public class PassCards
    {
        [Theory, AutoNSubstituteData]
        internal async Task pass_cards(
            [Substitute, Frozen] DaprWorkflowClient daprWorkflowClient,
            GameHub sut,
            Guid gameId,
            string workflowInstanceId,
            PassCard[] passCards)
        {
            // Arrange

            // Act

            await sut.PassCards(gameId, workflowInstanceId, passCards);

            // Assert

            Assert.NotNull(daprWorkflowClient);
        }
    }
}
