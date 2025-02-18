using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Api.Workflows;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hearts.Api.UnitTests.Workflows;

public class NotifyGameUpdatedActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_client_was_notified(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyGameUpdatedActivityInput notifyGameUpdatedActivityInput,
        NotifyGameUpdatedActivity sut)
    {
        // Arrange

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameUpdatedActivityInput);

        // Assert

        Assert.True(result.IsSuccess);

        await clientCallback.Received().GameUpdated(notifyGameUpdatedActivityInput.Game);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyGameUpdatedActivityInput notifyGameUpdatedActivityInput,
        NotifyGameUpdatedActivity sut)
    {
        // Arrange

        clientCallback.GameUpdated(notifyGameUpdatedActivityInput.Game)
            .ThrowsAsync<Exception>();

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameUpdatedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
