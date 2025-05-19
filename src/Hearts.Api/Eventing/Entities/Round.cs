namespace Hearts.Api.Eventing.Entities;

public record Round(Guid Id, Guid GameId)
{
    public Trick[] Tricks = [];

    public void AddTrick(Trick trick)
    {
        this.Tricks = [.. this.Tricks, trick];
    }
}
