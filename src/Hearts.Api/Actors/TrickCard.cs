using Hearts.Contracts;

namespace Hearts.Api.Actors;

public record TrickCard(Card Card, Guid PlayerId)
{
    public Contracts.TrickCard Map()
    {
        return new(this.Card, this.PlayerId);
    }
}
