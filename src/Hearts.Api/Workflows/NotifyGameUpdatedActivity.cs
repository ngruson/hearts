using Ardalis.Result;
using Dapr.Workflow;

namespace Hearts.Api.Workflows;

internal class NotifyGameUpdatedActivity(IClientCallback clientCallback) : WorkflowActivity<NotifyGameUpdatedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, NotifyGameUpdatedActivityInput input)
    {
        try
        {
            await clientCallback.GameUpdated(input.Game);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
