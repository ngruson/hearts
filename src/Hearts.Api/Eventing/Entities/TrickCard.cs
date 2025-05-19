using Hearts.Contracts;

namespace Hearts.Api.Eventing.Entities;

public record TrickCard(Guid PlayerId, Card Card);
