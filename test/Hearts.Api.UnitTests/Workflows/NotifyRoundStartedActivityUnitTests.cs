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

        notifyRoundStartedActivityInput = notifyRoundStartedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

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

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        clientCallback.RoundStarted(notifyRoundStartedActivityInput.Round)
            .ThrowsAsync<Exception>();

        notifyRoundStartedActivityInput = notifyRoundStartedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, notifyRoundStartedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
