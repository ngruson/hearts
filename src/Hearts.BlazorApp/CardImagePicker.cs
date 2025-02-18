using Hearts.Contracts;

namespace Hearts.BlazorApp;

static class CardImagePicker
{
    public static string GetCardImageUri(Card card)
    {
        return card.Rank switch
        {
            Rank.Two => GetCardImageUri(card.Suit, "2"),
            Rank.Three => GetCardImageUri(card.Suit, "3"),
            Rank.Four => GetCardImageUri(card.Suit, "4"),
            Rank.Five => GetCardImageUri(card.Suit, "5"),
            Rank.Six => GetCardImageUri(card.Suit, "6"),
            Rank.Seven => GetCardImageUri(card.Suit, "7"),
            Rank.Eight => GetCardImageUri(card.Suit, "8"),
            Rank.Nine => GetCardImageUri(card.Suit, "9"),
            Rank.Ten => GetCardImageUri(card.Suit, "10"),
            Rank.Jack => GetCardImageUri(card.Suit, "jack"),
            Rank.Queen => GetCardImageUri(card.Suit, "queen"),
            Rank.King => GetCardImageUri(card.Suit, "king"),
            Rank.Ace => GetCardImageUri(card.Suit, "ace"),
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetCardImageUri(Suit suit, string rank) =>
        $"/images/cards/{rank}_of_{suit.ToString().ToLower()}.svg";
}
