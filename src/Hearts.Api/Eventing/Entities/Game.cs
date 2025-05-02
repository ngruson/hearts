using Hearts.Contracts;

namespace Hearts.Api.Eventing.Entities;

internal class Game
{
    public Guid Id { get; set; }
    public Player[] Players { get; set; } = [];
    public Round[] Rounds { get; set; } = [];
}
