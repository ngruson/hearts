namespace Hearts.Api.Eventing.Events;

public record TrickStartedEvent(Guid Id, Guid RoundId);
