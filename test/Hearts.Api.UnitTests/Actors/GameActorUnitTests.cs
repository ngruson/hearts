using Dapr.Actors.Runtime;
using Hearts.Api.Actors;
using Hearts.Contracts;

namespace Hearts.Api.UnitTests.Actors;

public class GameActorUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task add_player(
        Player player)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        // Act

        await sut.AddPlayer(player);

        Game game = await sut.Map();

        // Assert

        Assert.Contains(player, game.Players);
    }

    [Fact]
    internal async Task add_bot_player()
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        // Act

        for (int i = 0; i < 4; i++)
        {
            await sut.AddBotPlayer();
        }

        Game game = await sut.Map();

        // Assert

        Assert.Equal(4, game.Players.Length);
    }

    public class StartNewRound
    {
        [Fact]
        internal async Task start_new_round_with_passing_direction_none()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            await sut.StartRound();

            Game game = await sut.Map();

            // Assert

            Assert.Equal(4, game.CurrentRound?.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, game.CurrentRound?.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_left()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            await sut.StartRound();

            Game game = await sut.Map();

            // Assert

            Assert.Equal(4, game.CurrentRound?.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, game.CurrentRound?.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_right()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartRound();
            await sut.StartRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            await sut.StartRound();

            Game game = await sut.Map();

            // Assert

            Assert.Equal(4, game.CurrentRound?.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, game.CurrentRound?.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_across()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartRound();
            await sut.StartRound();
            await sut.StartRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            await sut.StartRound();

            Game game = await sut.Map();

            // Assert

            Assert.Equal(4, game.CurrentRound?.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, game.CurrentRound?.Players.ElementAt(i).Cards.Length);
            }
        }        
    }

    [Fact]
    internal async Task map()
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        // Act            

        Game game = await sut.Map();

        // Assert

        Assert.NotNull(game);
    }

    [Theory, AutoNSubstituteData]
    internal async Task pass_cards(
        Player player1,
        Player player2,
        PassCard[] passCards)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        await sut.AddPlayer(player1);
        await sut.AddPlayer(player2);
        await sut.StartRound();

        passCards[0] = passCards[0] with { FromPlayerId = player1.Id };
        passCards[0] = passCards[0] with { ToPlayerId = player2.Id };

        // Act            

        await sut.PassCards([passCards[0]]);

        // Assert

        Assert.True(true);
    }
}
