using System.Diagnostics;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

record GameWorkflowInput(string TraceId, string SpanId, Player Player);
