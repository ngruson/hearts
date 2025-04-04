namespace Hearts.Api.Actors;

public record PlayerScore(Guid PlayerId, string PlayerName, int Points)
{
    public int Points { get; private set; } = Points;

    public Contracts.PlayerScore Map() => new(this.PlayerId, this.Points);
    public void SetPoints(int points) => this.Points = points;
}
