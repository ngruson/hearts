using Hearts.Api.Eventing.Entities;

namespace Hearts.Api.Eventing.Events;

internal record CardPlayedEvent(Guid RoundId, Guid TrickId, TrickCard TrickCard);
