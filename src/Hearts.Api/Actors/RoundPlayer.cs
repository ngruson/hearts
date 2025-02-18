using Hearts.Contracts;

namespace Hearts.Api.Actors;

public class RoundPlayer(Player player)
{
    public Player Player { get; set; } = player;
    public Card[] Cards { get; set; } = [];

    public Contracts.RoundPlayer Map()
    {
        return new(this.Player, this.Cards);
    }
}
