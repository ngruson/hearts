namespace Hearts.Api.Workflows;

record NotifyRoundStartedActivityInput(string TraceId, string SpanId, Contracts.Round Round);
