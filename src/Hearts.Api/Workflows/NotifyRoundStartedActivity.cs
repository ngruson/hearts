using Ardalis.Result;
using Dapr.Workflow;

namespace Hearts.Api.Workflows;

internal class NotifyRoundStartedActivity(IClientCallback clientCallback) : WorkflowActivity<NotifyRoundStartedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, NotifyRoundStartedActivityInput input)
    {
        try
        {
            await clientCallback.RoundStarted(input.Round);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
