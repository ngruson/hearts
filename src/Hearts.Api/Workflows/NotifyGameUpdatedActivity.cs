using System.Diagnostics;
using Ardalis.Result;
using Dapr.Workflow;
using Hearts.Api.OpenTelemetry;

namespace Hearts.Api.Workflows;

internal class NotifyGameUpdatedActivity(IClientCallback clientCallback, Instrumentation instrumentation) : WorkflowActivity<NotifyGameUpdatedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, NotifyGameUpdatedActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.ActivitySource.StartActivity(nameof(NotifyGameUpdatedActivity), ActivityKind.Internal, activityContext);

        try
        {
            await clientCallback.GameUpdated(input.Game);
            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            return Result.Error(ex.Message);
        }
    }
}
