using System.Diagnostics;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

record CreateNewGameActivityInput(string TraceId, string SpanId, Player Player);
