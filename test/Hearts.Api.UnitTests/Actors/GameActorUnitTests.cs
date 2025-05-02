using Ardalis.Result;
using Dapr.Actors.Runtime;
using Hearts.Api.Actors;
using Hearts.Contracts;
using Marten;

namespace Hearts.Api.UnitTests.Actors;

public class GameActorUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task add_bot_player(IDocumentStore documentStore)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host, documentStore);

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
        IDocumentStore documentStore,
        Player player)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host, documentStore);

        // Act

        await sut.AddPlayer(player);

        Game game = await sut.Map();

        // Assert

        Assert.Contains(player, game.Players);
    }

    [Theory, AutoNSubstituteData]
    internal async Task map(IDocumentStore documentStore)
    {
        // Arrange

        ActorHost host = ActorHost.CreateForTest<GameActor>();
        GameActor sut = new(host, documentStore);

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
            IDocumentStore documentStore,
            Player[] players)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

        [Theory, AutoNSubstituteData]
        internal async Task change_player_turn_given_no_current_round(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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
            IDocumentStore documentStore,
            Player player1,
            Player player2,
            PassCard[] passCards)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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
            IDocumentStore documentStore,
            Player player1,
            Player player2,
            PassCard[] passCards)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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
        [Theory, AutoNSubstituteData]
        internal async Task play_bots_when_current_round_is_not_null(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

        [Theory, AutoNSubstituteData]
        internal async Task do_not_play_bots_when_current_round_is_null(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            // Act

            await sut.PlayBots();

            // Assert

            Assert.Null(sut.CurrentRound);
        }
    }

    public class PlayCard
    {
        [Theory, AutoNSubstituteData]
        internal async Task play_card_when_current_round_is_not_null(
            IDocumentStore documentStore,
            Player player,
            Card card)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            await sut.AddPlayer(player);
            await sut.StartRound();

            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == Suit.Clubs && _.Rank == Rank.Two)))
            {
                sut.CurrentRound!.Players[0].Cards[0] = new Card(Suit.Clubs, Rank.Two);
            }
            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == card.Suit && _.Rank == card.Rank)))
            {
                sut.CurrentRound!.Players[0].Cards[1] = card;
            }
            
            await sut.StartTrick();

            //sut.CurrentRound!.CurrentTrick!.PlayerTurn = sut.CurrentRound.Players[0];

            // Act

            await sut.PlayCard(player.Id, card);

            // Assert

            Assert.Equal(12, sut.CurrentRound?.CurrentTrick?.PlayerTurn.Cards.Length);
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_play_card_when_current_round_is_null(
            IDocumentStore documentStore,
            Player player,
            Card card)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            // Act

            await sut.PlayCard(player.Id, card);

            // Assert

            Assert.Null(sut.CurrentRound);
        }
    }

    public class StartNewRound
    {
        [Theory, AutoNSubstituteData]
        internal async Task start_new_round_with_passing_direction_across(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

        [Theory, AutoNSubstituteData]
        internal async Task start_new_round_with_passing_direction_left(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

        [Theory, AutoNSubstituteData]
        internal async Task start_new_round_with_passing_direction_none(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

        [Theory, AutoNSubstituteData]
        internal async Task start_new_round_with_passing_direction_right(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

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

    public class StartTrick
    {
        [Theory, AutoNSubstituteData]
        internal async Task start_trick_when_current_round_is_not_null(
            IDocumentStore documentStore,
            Player[] players)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            foreach (Player player in players)
            {
                await sut.AddPlayer(player);
            }

            await sut.StartRound();

            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == Suit.Clubs && _.Rank == Rank.Two)))
            {
                sut.CurrentRound!.Players[0].Cards[0] = new Card(Suit.Clubs, Rank.Two);
            }

            // Act

            await sut.StartTrick();

            // Assert

            Assert.NotNull(sut.CurrentRound?.CurrentTrick);
        }

        [Theory, AutoNSubstituteData]
        internal async Task do_not_start_trick_when_current_round_is_null(
            IDocumentStore documentStore)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            // Act

            await sut.StartTrick();

            // Assert

            Assert.Null(sut.CurrentRound);
        }
    }

    public class ValidateCard
    {
        [Theory, AutoNSubstituteData]
        internal async Task return_success_when_played_card_is_valid(
            IDocumentStore documentStore,
            Player[] players)
        {
            // Arrange

            Card card = new(Suit.Clubs, Rank.Two);

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            foreach (Player player in players)
            {
                await sut.AddPlayer(player);
            }

            await sut.StartRound();

            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == Suit.Clubs && _.Rank == Rank.Two)))
            {
                sut.CurrentRound!.Players[0].Cards[0] = new Card(Suit.Clubs, Rank.Two);
            }
            if (!sut.CurrentRound!.Players.Any(_ => _.Cards.Any(_ => _.Suit == card.Suit && _.Rank == card.Rank)))
            {
                sut.CurrentRound!.Players[0].Cards[1] = card;
            }

            await sut.StartTrick();

            // Act

            
            Result result = await sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsSuccess);
        }

        [Theory, AutoNSubstituteData]
        internal async Task return_invalid_when_current_round_is_null(
            IDocumentStore documentStore,
            Card card)
        {
            // Arrange

            ActorHost host = ActorHost.CreateForTest<GameActor>();
            GameActor sut = new(host, documentStore);

            // Act

            Result result = await sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }
    }
}
