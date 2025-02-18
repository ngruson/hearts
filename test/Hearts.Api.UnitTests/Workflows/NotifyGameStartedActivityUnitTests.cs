using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Api.Workflows;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hearts.Api.UnitTests.Workflows;

public class NotifyGameStartedActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_client_was_notified(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyGameStartedActivityInput notifyGameStartedActivityInput,
        NotifyGameStartedActivity sut)
    {
        // Arrange

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameStartedActivityInput);

        // Assert

        Assert.True(result.IsSuccess);

        await clientCallback.Received().GameStarted(notifyGameStartedActivityInput.Game);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyGameStartedActivityInput notifyGameStartedActivityInput,
        NotifyGameStartedActivity sut)
    {
        // Arrange

        clientCallback.GameStarted(notifyGameStartedActivityInput.Game)
            .ThrowsAsync<Exception>();

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameStartedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
