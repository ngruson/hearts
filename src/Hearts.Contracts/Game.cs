namespace Hearts.Contracts;

public record Game(
    Guid Id,
    Player[] Players,    
    Round[] Rounds,
    PlayerScore[]? Scores = null,
    PassingDirection PassingDirection = PassingDirection.None,
    bool IsCompleted = false,
    string? Message = null)
{
    public Round? CurrentRound = Rounds.LastOrDefault();
}
