using System.Diagnostics;
using Ardalis.Result;
using Dapr.Workflow;
using Hearts.Api.OpenTelemetry;

namespace Hearts.Api.Workflows;

internal class NotifyRoundStartedActivity(IClientCallback clientCallback, Instrumentation instrumentation) : WorkflowActivity<NotifyRoundStartedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, NotifyRoundStartedActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.StartActivity(nameof(NotifyGameUpdatedActivity), ActivityKind.Internal, activityContext);

        try
        {
            await clientCallback.RoundStarted(input.Round);
            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            return Result.Error(ex.Message);
        }
    }
}
