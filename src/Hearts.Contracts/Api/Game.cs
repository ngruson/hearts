namespace Hearts.Contracts.Api;
public record Game(Guid Id, string Type, GameState State, int Enrolled);
