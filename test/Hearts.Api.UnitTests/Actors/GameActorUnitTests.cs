using Ardalis.Result;
using Dapr.Actors.Runtime;
using Hearts.Api.Actors;
using Hearts.Contracts;

namespace Hearts.Api.UnitTests.Actors;

public class GameActorUnitTests
{
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
    internal async Task map()
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host);

        await sut.StartRound();

        // Act

        Game game = await sut.Map();

        // Assert

        Assert.NotNull(game);
    }

    public class ChangePlayerTurn
    {
        [Theory, AutoNSubstituteData]
        internal async Task change_player_turn_given_current_round(
            Player[] players)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);            

            foreach (Player player in players)
            {
                await sut.AddPlayer(player);
            }

            await sut.StartRound();

            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == Suit.Clubs && _.Rank == Rank.Two)))
            {
                sut.CurrentRound!.Players[0].Cards[0] = new Card(Suit.Clubs, Rank.Two);
            }
            
            await sut.StartTrick();

            Api.Actors.RoundPlayer? turn = sut.CurrentRound?.CurrentTrick?.PlayerTurn;

            // Act

            await sut.ChangePlayerTurn();

            // Assert

            Assert.NotEqual(sut.CurrentRound?.CurrentTrick?.PlayerTurn.Player.Id, turn?.Player.Id);
        }

        [Fact]
        internal async Task change_player_turn_given_no_current_round()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            // Act

            await sut.ChangePlayerTurn();

            // Assert

            Assert.Null(sut.CurrentRound);
        }
    }

    public class PassCards
    {
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

            Assert.False(sut.CurrentRound?.SelectingCards);
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_pass_cards_given_no_current_round(
        Player player1,
        Player player2,
        PassCard[] passCards)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.AddPlayer(player1);
            await sut.AddPlayer(player2);

            passCards[0] = passCards[0] with { FromPlayerId = player1.Id };
            passCards[0] = passCards[0] with { ToPlayerId = player2.Id };

            // Act            

            await sut.PassCards([passCards[0]]);

            // Assert

            Assert.Null(sut.CurrentRound);
        }
    }

    public class PlayBots
    {
        [Fact]
        internal async Task play_bots()
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            await sut.AddBotPlayer();
            await sut.AddBotPlayer();
            await sut.AddBotPlayer();
            await sut.AddBotPlayer();

            await sut.StartRound();
            await sut.StartTrick();

            Api.Actors.RoundPlayer? turn = sut.CurrentRound?.CurrentTrick?.PlayerTurn;

            // Act

            await sut.PlayBots();

            // Assert

            Assert.Equal(12, turn?.Cards.Length);
            Assert.NotEqual(sut.CurrentRound?.CurrentTrick?.PlayerTurn.Player.Id, turn?.Player.Id);
        }
    }

    public class StartNewRound
    {
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
    }

    public class ValidateCard
    {
        [Theory, AutoNSubstituteData]
        internal async Task return_success_when_played_card_is_valid(
            Player[] players,
            Card card)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            foreach (Player player in players)
            {
                await sut.AddPlayer(player);
            }

            await sut.StartRound();

            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == Suit.Clubs && _.Rank == Rank.Two)))
            {
                sut.CurrentRound!.Players[0].Cards[0] = new Card(Suit.Clubs, Rank.Two);
            }

            await sut.StartTrick();

            // Act

            Result result = await sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsSuccess);
        }

        [Theory, AutoNSubstituteData]
        internal async Task return_invalid_when_current_round_is_null(
            Card card)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host);

            // Act

            Result result = await sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }
    }
}
