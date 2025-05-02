namespace Hearts.Contracts;

public record Round(Guid Id, Guid GameId, RoundPlayer[] Players, bool SelectingCards, Trick[] Tricks, PlayerScore[]? Scores, bool IsCompleted);
