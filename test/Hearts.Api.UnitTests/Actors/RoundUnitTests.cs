using Ardalis.Result;
using Hearts.Api.Actors;

namespace Hearts.Api.UnitTests.Actors;

public class RoundUnitTests
{
    public class ChangePlayerTurn
    {
        [Theory, AutoNSubstituteData]
        public void change_player_turn_given_current_trick_is_not_null(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            sut.Players = players;
            sut.CurrentTrick!.PlayerTurn = sut.Players[0];

            // Act

            sut.ChangePlayerTurn();

            // Assert

            Assert.Equal(sut.Players[1], sut.CurrentTrick?.PlayerTurn);
        }

        [Theory, AutoNSubstituteData]
        public void no_op_given_current_trick_is_null(
            Round sut)
        {
            // Arrange

            //sut.Players = players;
            sut.Tricks = [];

            // Act

            sut.ChangePlayerTurn();

            // Assert

            Assert.Empty(sut.Tricks);
        }
    }

    public class Create
    {
        [Theory, AutoNSubstituteData]
        internal void create_round_with_queen_of_spades(
            Guid gameId,
            Contracts.Player[] players)
        {
            // Arrange

            for (int i = 0; i < players.Length; i++)
            {
                players[0] = players[0] with { IsBot = true };
            }

            // Act

            Round result = Round.Create(gameId, players, true);

            // Assert

            Assert.Equal(gameId, result.GameId);
            Assert.Equal(players.Length, result.Players.Length);
        }
    }

    public class Map
    {
        [Theory, AutoNSubstituteData]
        public void map_incomplete_round_to_contracts_round(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            sut.Players = players;

            // Act

            Contracts.Round result = sut.Map();

            // Assert

            Assert.Equal(sut.GameId, result.GameId);
            Assert.Equal(sut.SelectingCards, result.SelectingCards);
            Assert.Equal(sut.IsCompleted, result.IsCompleted);
        }

        [Theory, AutoNSubstituteData]
        public void map_complete_round_to_contracts_round(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            sut.Players = players;

            foreach (Trick trick in sut.Tricks)
            {
                trick.IsCompleted = true;
            }

            while (!sut.IsCompleted)
            {
                sut.Tricks = [.. sut.Tricks, new Trick([.. players.Select(_ => _.Player)], players[0])];
                sut.Tricks[^1].IsCompleted = true;
            }

            // Act

            Contracts.Round result = sut.Map();

            // Assert

            Assert.Equal(sut.GameId, result.GameId);
            Assert.Equal(sut.SelectingCards, result.SelectingCards);
            Assert.Equal(sut.IsCompleted, result.IsCompleted);
        }
    }

    public class PlayBots
    {
        [Theory, AutoNSubstituteData]
        public void play_bots_when_current_trick_is_not_null(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            sut.Players = players;
            sut.CurrentTrick!.PlayerTurn = sut.Players[0];
            sut.CurrentTrick.IsCompleted = false;

            foreach (RoundPlayer player in sut.Players)
            {
                player.Player = player.Player with { IsBot = true };
            }

            // Act

            sut.PlayBots();

            // Assert

            Assert.Equal(2, sut.CurrentTrick!.PlayerTurn.Cards.Length);
        }

        [Theory, AutoNSubstituteData]
        public void no_op_when_current_trick_is_null(
            Round sut)
        {
            // Arrange

            sut.Tricks = [];

            // Act

            sut.PlayBots();

            // Assert

            Assert.Equal(3, sut.Players[0].Cards.Length);
        }
    }

    public class PlayCard
    {
        [Theory, AutoNSubstituteData]
        public void play_card_when_current_trick_is_not_null(
            Round sut,
            RoundPlayer[] players,            
            Contracts.Card card)
        {
            // Arrange

            players[0].Cards[0] = card;

            sut.Players = players;
            sut.CurrentTrick!.PlayerTurn = sut.Players[0];
            sut.CurrentTrick.IsCompleted = false;

            // Act

            sut.PlayCard(sut.Players[0].Player.Id, card);

            // Assert

            Assert.Equal(2, sut.CurrentTrick!.PlayerTurn.Cards.Length);
        }

        [Theory, AutoNSubstituteData]
        public void no_op_given_current_trick_is_null(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange

            sut.Tricks = [];

            // Act

            sut.PlayCard(players[0].Player.Id, card);

            // Assert

            Assert.Equal(3, sut.Players[0].Cards.Length);
        }

        [Theory, AutoNSubstituteData]
        public void break_hearts(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange

            card = card with { Suit = Contracts.Suit.Hearts };
            players[0].Cards[0] = card;

            sut.Players = players;
            sut.CurrentTrick!.PlayerTurn = sut.Players[0];
            sut.CurrentTrick.IsCompleted = false;
            sut.IsHeartsBroken = false;

            // Act

            sut.PlayCard(sut.Players[0].Player.Id, card);

            // Assert

            Assert.Equal(2, sut.CurrentTrick!.PlayerTurn.Cards.Length);
            Assert.True(sut.IsHeartsBroken);
        }

        [Theory, AutoNSubstituteData]
        public void no_op_when_player_not_found(
            Round sut,
            Guid playerId,
            Contracts.Card card)
        {
            // Arrange

            // Act

            sut.PlayCard(playerId, card);

            // Assert

            Assert.Equal(3, sut.Players[0].Cards.Length);
        }
    }

    public class StartTrick
    {
        [Theory, AutoNSubstituteData]
        public void start_trick_when_current_trick_is_not_null(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            sut.Players = players;
            sut.CurrentTrick!.Winner = sut.Players[0].Player;

            // Act

            sut.StartTrick();

            // Assert

            Assert.NotNull(sut.CurrentTrick);
        }

        [Theory, AutoNSubstituteData]
        public void no_op_when_current_trick_is_null(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            if (!players.Any(_ => _.Cards.Any(_ => _.Suit == Contracts.Suit.Clubs && _.Rank == Contracts.Rank.Two)))
            {
                players[0].Cards[0] = new Contracts.Card(Contracts.Suit.Clubs, Contracts.Rank.Two);
            }

            sut.Players = players;
            sut.Tricks = [];
            
            // Act

            sut.StartTrick();

            // Assert
            
            Assert.Single(sut.Tricks);
        }
    }

    public class ValidateCard
    {
        [Theory, AutoNSubstituteData]
        public void return_success_when_played_card_is_valid(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange

            sut.Players = players;
            sut.CurrentTrick!.PlayerTurn = sut.Players[0];
            sut.CurrentTrick.IsCompleted = false;
            sut.CurrentTrick.Suit = card.Suit;

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsSuccess);
        }

        [Theory, AutoNSubstituteData]
        public void return_invalid_when_current_trick_is_null(
            Round sut,
            Contracts.Card card)
        {
            // Arrange

            sut.Tricks = [];

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }

        [Theory, AutoNSubstituteData]
        public void return_invalid_when_two_of_clubs_is_not_played_on_first_trick(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange

            card = card with { Suit = Contracts.Suit.Hearts };

            if (!players.Any(_ => _.Cards.Any(_ => _.Suit == Contracts.Suit.Clubs && _.Rank == Contracts.Rank.Two)))
            {
                players[0].Cards[0] = new Contracts.Card(Contracts.Suit.Clubs, Contracts.Rank.Two);
            }

            sut.Players = players;
            sut.Tricks = [];
            sut.StartTrick();

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }

        [Theory, AutoNSubstituteData]
        public void return_invalid_when_wrong_suit_is_played(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange            

            if (!players.Any(_ => _.Cards.Any(_ => _.Suit == Contracts.Suit.Clubs && _.Rank == Contracts.Rank.Two)))
            {
                players[0].Cards[0] = new Contracts.Card(Contracts.Suit.Clubs, Contracts.Rank.Two);
            }

            sut.Players = players;
            sut.Tricks = [];
            sut.StartTrick();
            sut.CurrentTrick!.TrickCards = [new TrickCard(card, players[0].Player.Id)];
            card = card with { Suit = Contracts.Suit.Hearts };

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }

        [Theory, AutoNSubstituteData]
        public void return_invalid_when_hearts_is_played_on_first_trick(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange            

            if (!players.Any(_ => _.Cards.Any(_ => _.Suit == Contracts.Suit.Clubs && _.Rank == Contracts.Rank.Two)))
            {
                players[0].Cards[0] = new Contracts.Card(Contracts.Suit.Clubs, Contracts.Rank.Two);
            }

            sut.Players = players;
            sut.Tricks = [];
            sut.StartTrick();
            sut.CurrentTrick!.TrickCards = [new TrickCard(card, players[0].Player.Id)];
            card = card with { Suit = Contracts.Suit.Hearts };

            RoundPlayer roundPlayer = sut.Players.First(p => p.Player.Id == sut.CurrentTrick.PlayerTurn?.Player.Id);
            roundPlayer.Cards = [];

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }

        [Theory, AutoNSubstituteData]
        public void return_invalid_when_hearts_is_played_given_hearts_has_not_been_broken(
            Round sut,
            RoundPlayer[] players,
            Contracts.Card card)
        {
            // Arrange            

            if (!players.Any(_ => _.Cards.Any(_ => _.Suit == Contracts.Suit.Clubs && _.Rank == Contracts.Rank.Two)))
            {
                players[0].Cards[0] = new Contracts.Card(Contracts.Suit.Clubs, Contracts.Rank.Two);
            }

            sut.Players = players;
            sut.IsHeartsBroken = false;
            sut.CurrentTrick!.PlayerTurn = players[0];
            sut.CurrentTrick!.TrickCards = [];
            card = card with { Suit = Contracts.Suit.Hearts };

            RoundPlayer roundPlayer = sut.Players.First(p => p.Player.Id == sut.CurrentTrick.PlayerTurn?.Player.Id);
            roundPlayer.Cards = [new Contracts.Card(Contracts.Suit.Clubs, card.Rank)];

            // Act

            Result result = sut.ValidateCard(card);

            // Assert

            Assert.True(result.IsInvalid());
        }
    }

    public class Scores
    {
        [Theory, AutoNSubstituteData]
        public void calculate_scores(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange
            
            Trick trick = new([..players.Select(_ => _.Player)], players[0])
            {
                TrickCards = [..Enumerable.Repeat(
                    new TrickCard(new Contracts.Card(Contracts.Suit.Hearts, Contracts.Rank.Two), players[0].Player.Id),
                    1)],
                Winner = players[0].Player,
                IsCompleted = true
            };

            sut.Players = players;
            sut.Tricks = [.. Enumerable.Repeat(trick, 13)];

            // Act

            PlayerScore? result = sut.Scores?.Single(_ => _.PlayerId == players[0].Player.Id);

            // Assert

            Assert.Equal(13, result?.Points);
        }

        [Theory, AutoNSubstituteData]
        public void calculate_scores_with_queen_of_spades(
            Round sut,
            RoundPlayer[] players)
        {
            // Arrange

            Trick trick = new([.. players.Select(_ => _.Player)], players[0])
            {
                TrickCards = [..Enumerable.Repeat(
                    new TrickCard(new Contracts.Card(Contracts.Suit.Hearts, Contracts.Rank.Two), players[0].Player.Id),
                    1)],
                Winner = players[0].Player,
                IsCompleted = true
            };

            sut.Players = players;
            sut.Tricks = [.. Enumerable.Repeat(trick, 13)];
            sut.Tricks[0].TrickCards = [.. sut.Tricks[0].TrickCards,
                new TrickCard(new Contracts.Card(Contracts.Suit.Spades, Contracts.Rank.Queen), players[0].Player.Id)];

            // Act

            PlayerScore? result = sut.Scores?.Single(_ => _.PlayerId == players[0].Player.Id);

            // Assert

            Assert.Equal(0, result?.Points);

            foreach (PlayerScore? playerScore in sut.Scores!.Where(_ => _.PlayerId != players[0].Player.Id))
            {
                Assert.Equal(26, playerScore?.Points);
            }
        }
    }
}
