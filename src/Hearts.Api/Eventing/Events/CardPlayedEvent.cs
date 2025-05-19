using Hearts.Api.Eventing.Entities;

namespace Hearts.Api.Eventing.Events;

public record CardPlayedEvent(Guid RoundId, Guid TrickId, TrickCard TrickCard);
