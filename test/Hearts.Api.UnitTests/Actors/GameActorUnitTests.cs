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

    [Fact]
    internal async Task start_new_round()
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
}
