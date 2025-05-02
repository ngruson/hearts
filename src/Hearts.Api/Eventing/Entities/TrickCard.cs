using Hearts.Contracts;

namespace Hearts.Api.Eventing.Entities;

internal record TrickCard(Guid PlayerId, Card Card);
