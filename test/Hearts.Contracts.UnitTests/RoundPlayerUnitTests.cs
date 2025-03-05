namespace Hearts.Contracts.UnitTests;

public class RoundPlayerUnitTests
{
    [Theory, AutoNSubstituteData]
    internal void add_card(
        RoundPlayer sut,
        Card card)
    {
        // Arrange

        // Act

        sut.AddCard(card);

        // Assert

        Assert.Contains(card, sut.Cards);
    }

    [Theory, AutoNSubstituteData]
    internal void remove_card(
        RoundPlayer sut,
        Card card)
    {
        // Arrange

        sut.AddCard(card);

        // Act

        sut.RemoveCard(card);

        // Assert

        Assert.DoesNotContain(card, sut.Cards);
    }
}
