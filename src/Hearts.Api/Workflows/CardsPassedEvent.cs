using Hearts.Contracts;

namespace Hearts.Api.Workflows;

public record CardsPassedEvent(Guid GameId, PassCard[] CardsPassed);
