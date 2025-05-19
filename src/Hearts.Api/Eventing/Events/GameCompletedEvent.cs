namespace Hearts.Api.Eventing.Events;

public record GameCompletedEvent(Guid GameId, DateTime CompletedTimestamp);
