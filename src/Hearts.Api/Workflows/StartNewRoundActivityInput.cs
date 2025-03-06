namespace Hearts.Api.Workflows;

record StartNewRoundActivityInput(string TraceId, string SpanId, Guid GameId);
