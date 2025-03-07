using Hearts.Contracts;

namespace Hearts.Api.Workflows;

record NotifyGameUpdatedActivityInput(string TraceId, string SpanId, Game Game);
