namespace Hearts.Contracts.Events;

public record InvalidCardPlayedEvent(Card Card, string? Message);
