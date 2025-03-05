namespace Hearts.Contracts;

public record PassCard(Guid FromPlayerId, Guid ToPlayerId, Card Card);
