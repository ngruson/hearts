namespace Hearts.Contracts;

public record Turn(Guid PlayerId, string PlayerName, bool IsBot);
