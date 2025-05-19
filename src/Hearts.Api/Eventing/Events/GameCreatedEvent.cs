namespace Hearts.Api.Eventing.Events;

public record GameCreatedEvent(Guid GameId, DateTime CreatedTimestamp);
