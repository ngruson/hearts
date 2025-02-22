using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Api.Workflows;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hearts.Api.UnitTests.Workflows;

public class NotifyRoundStartedActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_client_was_notified(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyRoundStartedActivityInput notifyRoundStartedActivityInput,
        NotifyRoundStartedActivity sut)
    {
        // Arrange

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyRoundStartedActivityInput);

        // Assert

        Assert.True(result.IsSuccess);

        await clientCallback.Received().RoundStarted(notifyRoundStartedActivityInput.Round);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyRoundStartedActivityInput notifyRoundStartedActivityInput,
        NotifyRoundStartedActivity sut)
    {
        // Arrange

        clientCallback.RoundStarted(notifyRoundStartedActivityInput.Round)
            .ThrowsAsync<Exception>();

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyRoundStartedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
