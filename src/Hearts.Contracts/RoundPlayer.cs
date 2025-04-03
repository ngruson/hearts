namespace Hearts.Contracts;

public class RoundPlayer(Player player, Card[] cards, Card[] cardsToPass)
{
    public Player Player { get; } = player;
    public Card[] Cards { get; private set; } = cards;
    public Card[] CardsToPass { get; set; } = cardsToPass;

    public void AddCard(Card card)
    {        
        this.Cards = [.. this.Cards, card];
    }

    public void RemoveCard(Card card)
    {
        this.Cards = [.. this.Cards.Where(c => c.Suit != card.Suit && c.Rank != card.Rank)];
    }
}
