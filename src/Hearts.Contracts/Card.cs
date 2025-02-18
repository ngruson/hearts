namespace Hearts.Contracts;

public class Card(Suit suit, Rank rank)
{
    public Suit Suit => suit;
    public Rank Rank => rank;
}
