namespace Hearts.Contracts;

public record Player(Guid Id, string PlayerName);
public record BotPlayer(Guid Id, string PlayerName) : Player(Id, PlayerName);
