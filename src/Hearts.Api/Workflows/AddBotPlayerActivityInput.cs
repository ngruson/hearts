namespace Hearts.Api.Workflows;

record AddBotPlayerActivityInput(string TraceId, string SpanId, Guid GameId);
