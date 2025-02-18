using Ardalis.Result;
using Dapr.Workflow;

namespace Hearts.Api.Workflows;

internal class NotifyGameStartedActivity(IClientCallback clientCallback) : WorkflowActivity<NotifyGameStartedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, NotifyGameStartedActivityInput input)
    {
        try
        {
            await clientCallback.GameStarted(input.Game);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
