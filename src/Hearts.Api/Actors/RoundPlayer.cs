using Hearts.Contracts;

namespace Hearts.Api.Actors;

public class RoundPlayer(Player player)
{
    public Player Player { get; set; } = player;
    public Card[] Cards { get; set; } = [];
    public Card[] CardsToPass { get; set; } = [];

    public Contracts.RoundPlayer Map()
    {
        return new(this.Player, this.Cards, this.CardsToPass);
    }

    internal void PlayCard(Card card, Trick? trick)
    {
        Card cardToDelete = this.Cards.Single(c => c.Suit == card.Suit && c.Rank == card.Rank);
        this.Cards = [.. this.Cards.Except([cardToDelete])];
        trick?.AddCard(card, this.Player.Id);
    }

    internal Card? SelectCardToPlay(Trick trick, bool heartsBroken)
    {
        if (this.Player.IsBot is false)
        {
            return null;
        }

        Card[] matchingCards = [.. this.Cards.Where(card => card.Suit == trick.Suit)];
        Card? cardToPlay = null;

        if (matchingCards.Length > 0)
        {
            cardToPlay = matchingCards.OrderBy(card => card.Rank).First();
        }
        else if (this.Cards.Length > 0)
        {
            Card? queenOfSpades = this.Cards.SingleOrDefault(_ => _.Suit == Suit.Spades && _.Rank == Rank.Queen);
            if (queenOfSpades is not null && trick.TrickCards.Length > 0)
            {
                cardToPlay = queenOfSpades;
            }
            else if (heartsBroken || this.Cards.Any(_ => _.Suit != Suit.Hearts) is false)
            {
                cardToPlay = this.Cards
                    .OrderBy(card => card.Rank)
                    .First();
            }
            else
            {
                cardToPlay = this.Cards
                    .Where(_ => _.Suit != Suit.Hearts)
                    .OrderBy(card => card.Rank)
                    .First();
            }
        }

        return cardToPlay;
    }
}
