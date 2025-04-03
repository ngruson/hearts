using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using OpenTelemetry;
using OpenTelemetry.Trace;

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
            GameHub sut,
            Player player)
        {
            // Arrange

            using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("Hearts.Api")
                .Build();

            // Act

            await sut.CreateNewGame(player);

            // Assert
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_create_new_game_when_activity_is_null(
            GameHub sut,
            Player player)
        {
            // Arrange

            // Act

            await sut.CreateNewGame(player);

            // Assert
        }
    }

    public class PassCards
    {
        [Theory, AutoNSubstituteData]
        internal async Task pass_cards(
            GameHub sut,
            Guid gameId,
            PassCard[] passCards)
        {
            // Arrange

            // Act

            await sut.PassCards(gameId, passCards);

            // Assert
        }
    }
}
