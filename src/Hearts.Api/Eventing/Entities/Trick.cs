namespace Hearts.Api.Eventing.Entities;

internal record Trick(Guid Id)
{
    public TrickCard[] Cards = [];
}
