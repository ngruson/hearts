using Hearts.Contracts;

namespace Hearts.Api.Actors;

public class Round
{
    private readonly Trick[] tricks = [];

    public RoundPlayer[] Players { get; set; } = [];

    public Contracts.Round Map()
    {
        return new([.. this.Players.Select(player => player.Map())]);
    }

    public static Round Create(Player[] players)
    {
        Round round = new()
        {
            Players = [.. players.Select(player => new RoundPlayer(player))]
        };

        round.DealCards();

        return round;
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
