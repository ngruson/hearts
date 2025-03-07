using System.Diagnostics;
using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Api.Workflows;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenTelemetry;
using OpenTelemetry.Trace;

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

        notifyGameUpdatedActivityInput = notifyGameUpdatedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

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

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        clientCallback.GameUpdated(notifyGameUpdatedActivityInput.Game)
            .ThrowsAsync<Exception>();

        notifyGameUpdatedActivityInput = notifyGameUpdatedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameUpdatedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown_given_no_activity(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IClientCallback clientCallback,
        NotifyGameUpdatedActivityInput notifyGameUpdatedActivityInput,
        NotifyGameUpdatedActivity sut)
    {
        // Arrange

        clientCallback.GameUpdated(notifyGameUpdatedActivityInput.Game)
            .ThrowsAsync<Exception>();

        notifyGameUpdatedActivityInput = notifyGameUpdatedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyGameUpdatedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
