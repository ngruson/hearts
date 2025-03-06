namespace Hearts.Api.Workflows;

public record CardsPassedActivityInput(string TraceId, string SpanId, CardsPassedEvent CardsPassedEvent);
