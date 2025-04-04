using System.Diagnostics;
using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Actors;
using Dapr.Actors.Client;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;
using Hearts.Contracts;
using Hearts.Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
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
            [Substitute, Frozen] Instrumentation instrumentation,
            Activity activity,
            GameHub sut,
            string playerName)
        {
            // Arrange

            instrumentation.StartActivity(nameof(CreatePlayer)).Returns(activity);

            // Act

            await sut.CreatePlayer(playerName);

            // Assert

            await hubCallerClients.Caller.Received().PlayerCreated(Arg.Any<Player>());
        }

        [Theory, AutoNSubstituteData]
        internal async Task return_when_activity_is_null(
            [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
            GameHub sut,
            string playerName)
        {
            // Arrange

            // Act

            await sut.CreatePlayer(playerName);

            // Assert

            await hubCallerClients.Caller.DidNotReceive().PlayerCreated(Arg.Any<Player>());
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

        [Theory, AutoNSubstituteData]
        internal async Task add_exception_to_trace_when_exception_is_thrown(
            [Substitute, Frozen] IHubCallerClients<IGameClient> hubCallerClients,
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            GameHub sut,
            string playerName)
        {
            // Arrange

            instrumentation.StartActivity(nameof(CreatePlayer)).Returns(activity);

            hubCallerClients.Caller.PlayerCreated(Arg.Any<Player>())
                .ThrowsAsync<Exception>();

            // Act

            await sut.CreatePlayer(playerName);

            // Assert

            await hubCallerClients.Caller.Received().PlayerCreated(Arg.Any<Player>());
        }
    }

    public class CreateNewGame
    {
        [Theory, AutoNSubstituteData]
        internal async Task create_new_game(
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            Game game,
            GameHub sut,
            Player player)
        {
            // Arrange

            gameActor.Map().Returns(game);

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("Hearts.Api")
                .Build();

            // Act

            await sut.CreateNewGame(player);

            // Assert

            await gameActor.Received().AddPlayer(player);
            await gameActor.Received().StartRound();
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_create_new_game_when_activity_is_null(
            [Substitute, Frozen] IGameActor gameActor,
            GameHub sut,
            Player player)
        {
            // Arrange

            // Act

            await sut.CreateNewGame(player);

            // Assert

            await gameActor.DidNotReceive().AddPlayer(player);
            await gameActor.DidNotReceive().StartRound();
        }
    }

    public class PassCards
    {
        [Theory, AutoNSubstituteData]
        internal async Task pass_cards(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            PassCard[] passCards)
        {
            // Arrange

            instrumentation.StartActivity(nameof(PassCards)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            gameActor.Map().Returns(game);

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            // Act

            await sut.PassCards(game.Id, passCards);

            // Assert

            await gameActor.Received().PassCards(passCards);
            await gameActor.Received().StartTrick();
            await gameActor.Received().PlayBots();
        }

        [Theory, AutoNSubstituteData]
        internal async Task return_when_no_activity(
            [Substitute, Frozen] IGameActor gameActor,
            GameHub sut,
            Game game,
            PassCard[] passCards)
        {
            // Arrange

            // Act

            await sut.PassCards(game.Id, passCards);

            // Assert

            await gameActor.DidNotReceive().PassCards(passCards);
            await gameActor.DidNotReceive().StartTrick();
            await gameActor.DidNotReceive().PlayBots();
        }

        [Theory, AutoNSubstituteData]
        internal async Task add_exception_to_trace_when_exception_is_thrown(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            PassCard[] passCards)
        {
            // Arrange

            instrumentation.StartActivity(nameof(PassCards)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            gameActor.Map().Returns(game);

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            gameActor.PassCards(passCards).ThrowsAsync<Exception>();

            // Act

            await sut.PassCards(game.Id, passCards);

            // Assert

            await gameActor.Received().PassCards(passCards);
            await gameActor.DidNotReceive().StartTrick();
            await gameActor.DidNotReceive().PlayBots();
        }
    }

    public class PlayCard
    {
        [Theory, AutoNSubstituteData]
        internal async Task play_card_when_round_is_completed(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            Guid playerId,
            Card card)
        {
            // Arrange
            instrumentation.StartActivity(nameof(PlayCard)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            game = game with { PassingDirection = PassingDirection.None };
            game.CurrentRound = game.CurrentRound! with { IsCompleted = true };
            gameActor.Map().Returns(game);
            gameActor.ValidateCard(card).Returns(Result.Success());

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            // Act

            await sut.PlayCard(game.Id, playerId, card);

            // Assert

            await sut.Clients.Caller.Received(2).GameUpdated(Arg.Any<Game>());
            await gameActor.Received().StartRound();
            await gameActor.Received(2).PlayBots();
            await gameActor.Received().StartTrick();
        }

        [Theory, AutoNSubstituteData]
        internal async Task play_card_when_last_in_trick(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            Guid playerId,
            Card card)
        {
            // Arrange
            instrumentation.StartActivity(nameof(PlayCard)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            gameActor.Map().Returns(game);
            gameActor.ValidateCard(card).Returns(Result.Success());

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            // Act

            await sut.PlayCard(game.Id, playerId, card);

            // Assert

            await sut.Clients.Caller.Received(4).GameUpdated(Arg.Any<Game>());
            await gameActor.Received().PlayCard(playerId, card);
            await gameActor.Received(2).PlayBots();
            await gameActor.Received().StartTrick();
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_play_card_given_invalid_card(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            Guid playerId,
            Card card)
        {
            // Arrange
            instrumentation.StartActivity(nameof(PlayCard)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            gameActor.Map().Returns(game);
            gameActor.ValidateCard(card).Returns(Result.Error());

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            // Act
            
            await sut.PlayCard(game.Id, playerId, card);

            // Assert
            
            await sut.Clients.Caller.Received().InvalidCardPlayed(Arg.Any<InvalidCardPlayedEvent>());
            await sut.Clients.Caller.DidNotReceive().GameUpdated(Arg.Any<Game>());
            await gameActor.DidNotReceive().PlayCard(playerId, card);
            await gameActor.DidNotReceive().PlayBots();
            await gameActor.DidNotReceive().StartTrick();
        }

        [Theory, AutoNSubstituteData]
        internal async Task add_exception_to_trace_when_exception_is_thrown(
            [Substitute, Frozen] Instrumentation instrumentation,
            [Substitute, Frozen] TestActivity activity,
            [Substitute, Frozen] IGameActor gameActor,
            [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
            GameHub sut,
            Game game,
            Guid playerId,
            Card card)
        {
            // Arrange
            instrumentation.StartActivity(nameof(PlayCard)).Returns(activity);
            instrumentation.StartActivity(nameof(IGameActor.StartTrick)).Returns(activity);

            gameActor.Map().Returns(game);
            gameActor.ValidateCard(card).ThrowsAsync<Exception>();

            actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
                .Returns(gameActor);

            // Act

            await sut.PlayCard(game.Id, playerId, card);

            // Assert

            await gameActor.DidNotReceive().PlayCard(playerId, card);
            await gameActor.DidNotReceive().PlayBots();
            await gameActor.DidNotReceive().StartTrick();
        }
    }
}
