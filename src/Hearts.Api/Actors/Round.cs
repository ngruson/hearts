using Ardalis.Result;
using Hearts.Contracts;

namespace Hearts.Api.Actors;

public class Round
{
    public Trick? CurrentTrick => this.Tricks.LastOrDefault();
    public Guid GameId { get; set; }
    public Guid Id { get; set; }
    public bool IsCompleted => this.Tricks.Length == 13 && this.Tricks.All(_ => _.IsCompleted);
    public bool IsHeartsBroken { get; set; }
    public RoundPlayer[] Players { get; set; } = [];
    public PlayerScore[]? Scores => this.CalculateScore();
    public bool SelectingCards { get; set; } = true;
    public Trick[] Tricks { get; set; } = [];
    internal static Round Create(Guid gameId, Player[] players, bool selectingCards)
    {
        Round round = new()
        {
            Id = Guid.CreateVersion7(),
            GameId = gameId,
            Players = [.. players.Select(player => new RoundPlayer(player))],
            SelectingCards = selectingCards
        };

        round.DealCards();

        if (selectingCards)
        {
            round.SelectCardsToPass();
        }

        return round;
    }

    internal void ChangePlayerTurn()
    {
        if (this.CurrentTrick is null)
        {
            return;
        }

        RoundPlayer roundPlayer = this.Players[(Array.IndexOf(this.Players, this.Players.First(_ => _.Player.Id == this.CurrentTrick?.PlayerTurn?.Player.Id)) + 1) % this.Players.Length];
        this.CurrentTrick.SetPlayerTurn(roundPlayer);
    }

    internal Contracts.Round Map()
    {
        return new(
            this.Id,
            this.GameId,
            [.. this.Players.Select(_ => _.Map())],
            this.SelectingCards,
            [.. this.Tricks.Select(_ => _.Map())],
            this.Scores is not null ? [.. this.Scores.Select(_ => _.Map())] : null,
            this.IsCompleted);
    }

    internal Round PassCards(PassCard[] passCards)
    {
        foreach (RoundPlayer roundPlayer in this.Players)
        {
            roundPlayer.Cards = [.. roundPlayer.Cards, .. passCards.Where(_ => _.ToPlayerId == roundPlayer.Player.Id).Select(_ => _.Card)];
            roundPlayer.Cards = [.. roundPlayer.Cards.Where(_ => !passCards.Any(p => p.FromPlayerId == roundPlayer.Player.Id && p.Card.Suit == _.Suit && p.Card.Rank == _.Rank))];
            roundPlayer.CardsToPass = [];
        }

        this.SortPlayerCards();
        this.SelectingCards = false;

        return this;
    }

    internal void PlayBots()
    {
        if (this.CurrentTrick is null)
        {
            return;
        }

        while (this.CurrentTrick.PlayerTurn.Player.IsBot == true && this.CurrentTrick.PlayerTurn.Cards.Length > 0 && this.CurrentTrick.IsCompleted is false)
        {
            Card? card = this.CurrentTrick.PlayerTurn.SelectCardToPlay(this.CurrentTrick, this.IsHeartsBroken);
            this.PlayCard(this.CurrentTrick.PlayerTurn.Player.Id, card!);
        }
    }

    internal void PlayCard(Guid playerId, Card card)
    {
        if (this.CurrentTrick is null)
        {
            return;
        }

        RoundPlayer? roundPlayer = this.Players.SingleOrDefault(p => p.Player.Id == playerId);

        roundPlayer?.PlayCard(card, this.CurrentTrick);

        if (this.IsHeartsBroken is false && card.Suit == Suit.Hearts)
        {
            this.IsHeartsBroken = true;
        }

        this.CurrentTrick.CheckCompleted();

        if (this.CurrentTrick.IsCompleted == false)
        {
            this.ChangePlayerTurn();
        }
    }

    internal void StartTrick()
    {
        Trick? trick = null;

        if (this.Tricks.Length > 0)
        {
            trick = Trick.Create(
                this.Id,
                [.. this.Players.Select(_ => _.Player)],
                this.Players.Single(_ => _.Player.Id == this.CurrentTrick?.Winner?.Id),
                null);
        }
        else
        {
            trick = Trick.Create(
                this.Id,
                [.. this.Players.Select(_ => _.Player)],
                this.Players.Single(_ => _.Cards.Any(card => card.Suit == Suit.Clubs && card.Rank == Rank.Two)),
                Suit.Clubs);
        }

        this.Tricks = [.. this.Tricks, trick];
    }

    internal Result ValidateCard(Card card)
    {
        if (this.CurrentTrick is null)
        {
            return Result.Invalid(new ValidationError("Trick is null"));
        }

        if (this.Tricks.Length == 1 && this.CurrentTrick.TrickCards.Length == 0 && !(card.Suit == Suit.Clubs && card.Rank == Rank.Two))
        {
            return Result.Invalid(new ValidationError("The 2 of clubs must be played on the first trick"));
        }

        RoundPlayer roundPlayer = this.Players.First(p => p.Player.Id == this.CurrentTrick.PlayerTurn?.Player.Id);

        if (roundPlayer.Cards.Any(_ => _.Suit == this.CurrentTrick.Suit) && card.Suit != this.CurrentTrick.Suit)
        {
            return Result.Invalid(new ValidationError($"The suit of the current trick is {this.CurrentTrick.Suit.ToString()!.ToLower()}"));
        }

        if (this.Tricks.Length == 1 && card.Suit == Suit.Hearts)
        {
            return Result.Invalid(new ValidationError("Hearts cannot be played on the first trick"));
        }

        if ((this.Tricks.Length > 1 && this.CurrentTrick.TrickCards.Length == 0)
            && roundPlayer.Cards.Any(_ => _.Suit != Suit.Hearts)
            && this.IsHeartsBroken is false
            && card.Suit == Suit.Hearts)
        {
            return Result.Invalid(new ValidationError("Hearts have not been broken"));
        }

        return Result.Success();
    }

    private PlayerScore[]? CalculateScore()
    {
        if (this.IsCompleted)
        {
            PlayerScore[] scores = [];

            (Player Winner, Card[] Cards)[] cardsByWinner = [.. this.Tricks
                .Where(trick => trick.Winner != null)
                .SelectMany(trick => trick.TrickCards, (trick, trickCard) => new { trick.Winner, trickCard.Card })
                .GroupBy(x => x.Winner)
                .Select(group => (Winner: group.Key!, Cards: group.Select(x => x.Card).ToArray()))];

            foreach (RoundPlayer player in this.Players)
            {
                int score = 0;

                (Player Winner, Card[] Cards)? winner = cardsByWinner.SingleOrDefault(_ => _.Winner.Id == player.Player.Id);

                if (cardsByWinner.Any(entry => entry.Winner.Id == player.Player.Id))
                {
                    score = winner.Value.Cards.Count(card => card.Suit == Suit.Hearts);

                    if (winner.Value.Cards.Any(_ => _.Suit == Suit.Spades && _.Rank == Rank.Queen))
                    {
                        score += 13;
                    }
                }
               
                scores = [.. scores, new PlayerScore(player.Player.Id, player.Player.PlayerName, score)];
            }

            PlayerScore? shotTheMoon = scores.SingleOrDefault(_ => _.Points == 26);

            if (shotTheMoon is not null)
            {
                shotTheMoon.SetPoints(0);

                foreach (PlayerScore? player in scores.Where(_ => _.PlayerId != shotTheMoon.PlayerId))
                {
                    player.SetPoints(26);
                }
            }

            return scores;
        }
        else
        {
            return null;
        }
    }

    private void DealCards()
    {
        List<Card> deck = [];

        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                deck.Add(new Card(suit, rank));
            }
        }

        Random random = new();
        deck = [.. deck.OrderBy(_ => random.Next())];

        for (int i = 0; i < 13; i++)
        {
            foreach (RoundPlayer player in this.Players)
            {
                player.Cards = [.. player.Cards, deck[i * this.Players.Length + Array.IndexOf(this.Players, player)]];
            }
        }

        this.SortPlayerCards();
            
    }
    private void SelectCardsToPass()
    {
        foreach (RoundPlayer player in this.Players.Where(_ => _.Player.IsBot))
        {
            Card[] cardsToPass = [.. player.Cards.Take(3)];

            if (player.Cards.Any(_ => _.Suit == Suit.Spades && _.Rank == Rank.Queen)
                && !cardsToPass.Any(_ => _.Suit == Suit.Spades && _.Rank == Rank.Queen))
            {
                cardsToPass[0] = player.Cards.Single(_ => _.Suit == Suit.Spades && _.Rank == Rank.Queen);
            }

            player.CardsToPass = cardsToPass;
        }
    }

    private void SortPlayerCards()
    {
        foreach (RoundPlayer player in this.Players)
        {
            player.Cards = [.. player.Cards
                .OrderBy(card => card, new CardComparer())];
        }
    }
    private class CardComparer : IComparer<Card>
    {
        public int Compare(Card? x, Card? y)
        {
            if (x!.Suit != y!.Suit)
            {
                return GetSuitOrder(x.Suit).CompareTo(GetSuitOrder(y.Suit));
            }
            return x.Rank.CompareTo(y.Rank);
        }

        private static int GetSuitOrder(Suit? suit)
        {
        
            return suit switch
            {
                Suit.Hearts => 0,
                Suit.Spades => 1,
                Suit.Diamonds => 2,
                Suit.Clubs => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
            };
        }
    }
}
