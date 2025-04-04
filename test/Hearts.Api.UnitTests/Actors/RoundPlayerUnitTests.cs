using Hearts.Api.Actors;

namespace Hearts.Api.UnitTests.Actors;

public class RoundPlayerUnitTests
{
    [Theory, AutoNSubstituteData]
    public void select_card_to_play_given_player_is_not_a_bot(
        RoundPlayer sut,
        Trick trick)
    {
        // Arrange

        // Act

        Contracts.Card? card = sut.SelectCardToPlay(trick, false);

        // Assert

        Assert.Null(card);
    }

    [Theory, AutoNSubstituteData]
    public void select_card_to_play_when_matching_card_found(
        RoundPlayer sut,
        Trick trick,
        Contracts.Card card)
    {
        // Arrange

        card = card with { Suit = trick.Suit!.Value };
        sut.Player = sut.Player with { IsBot = true };
        sut.Cards = [card];

        // Act

        Contracts.Card? selectedCard = sut.SelectCardToPlay(trick, false);

        // Assert

        Assert.Equal(card, selectedCard);
    }

    [Theory, AutoNSubstituteData]
    public void select_card_to_play_when_no_matching_card_and_queen_of_spades_found(
        RoundPlayer sut,
        Trick trick)
    {
        // Arrange

        trick.Suit = Contracts.Suit.Hearts;
        Contracts.Card card = new(Contracts.Suit.Spades, Contracts.Rank.Queen);
        sut.Player = sut.Player with { IsBot = true };
        sut.Cards = [card];

        // Act

        Contracts.Card? selectedCard = sut.SelectCardToPlay(trick, false);

        // Assert

        Assert.Equal(card, selectedCard);
    }

    [Theory, AutoNSubstituteData]
    public void select_card_to_play_when_no_matching_card_and_hearts_found_given_hearts_broken(
        RoundPlayer sut,
        Trick trick)
    {
        // Arrange

        trick.Suit = Contracts.Suit.Clubs;
        Contracts.Card card = new(Contracts.Suit.Hearts, Contracts.Rank.Queen);
        sut.Player = sut.Player with { IsBot = true };
        sut.Cards = [card];

        // Act

        Contracts.Card? selectedCard = sut.SelectCardToPlay(trick, true);

        // Assert

        Assert.Equal(card, selectedCard);
    }

    [Theory, AutoNSubstituteData]
    public void select_card_to_play_when_no_matching_card_given_hearts_not_broken(
        RoundPlayer sut,
        Trick trick)
    {
        // Arrange

        trick.Suit = Contracts.Suit.Clubs;
        Contracts.Card card = new(Contracts.Suit.Diamonds, Contracts.Rank.Queen);
        sut.Player = sut.Player with { IsBot = true };
        sut.Cards = [card];

        // Act

        Contracts.Card? selectedCard = sut.SelectCardToPlay(trick, false);

        // Assert

        Assert.Equal(card, selectedCard);
    }
}
