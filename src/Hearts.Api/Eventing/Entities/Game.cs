using Hearts.Contracts;

namespace Hearts.Api.Eventing.Entities;

public class Game
{
    public Guid Id { get; set; }
    public GameState State { get; set; }
    public Player[] Players { get; set; } = [];
    public Round[] Rounds { get; set; } = [];
}
