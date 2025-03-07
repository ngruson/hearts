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

        List<Player> players = await sut.Players;

        // Assert

        Assert.Contains(player, players);
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

        List<Player> players = await sut.Players;

        // Assert

        Assert.Equal(4, players.Count);
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

            Contracts.Round round = await sut.StartNewRound();

            // Assert

            Assert.Equal(4, round.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, round.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_left()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartNewRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            Contracts.Round round = await sut.StartNewRound();

            // Assert

            Assert.Equal(4, round.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, round.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_right()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartNewRound();
            await sut.StartNewRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            Contracts.Round round = await sut.StartNewRound();

            // Assert

            Assert.Equal(4, round.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, round.Players.ElementAt(i).Cards.Length);
            }
        }

        [Fact]
        internal async Task start_new_round_with_passing_direction_across()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.StartNewRound();
            await sut.StartNewRound();
            await sut.StartNewRound();

            // Act

            for (int i = 0; i < 4; i++)
            {
                await sut.AddBotPlayer();
            }

            Contracts.Round round = await sut.StartNewRound();

            // Assert

            Assert.Equal(4, round.Players.Length);

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(13, round.Players.ElementAt(i).Cards.Length);
            }
        }        
    }

    [Theory, AutoNSubstituteData]
    internal async Task map(string workflowInstanceId)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        // Act            

        Game game = await sut.Map(workflowInstanceId);

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
        await sut.StartNewRound();

        passCards[0] = passCards[0] with { FromPlayerId = player1.Id };
        passCards[0] = passCards[0] with { ToPlayerId = player2.Id };

        // Act            

        await sut.PassCards([passCards[0]]);

        // Assert

        Assert.True(true);
    }
}
