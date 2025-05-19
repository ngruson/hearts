namespace Hearts.Api.Eventing.Entities;

public record Trick(Guid Id)
{
    public TrickCard[] Cards = [];
}
