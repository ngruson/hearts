using System.Diagnostics;
using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.Workflows;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Hearts.Api.UnitTests.Workflows;

public class CardsPassedActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_pass_cards_completes(
        [Substitute, Frozen] WorkflowActivityContext workflowActivityContext,
        CardsPassedActivity sut,
        CardsPassedActivityInput cardsPassedActivityInput)
    {
        // Arrange

        cardsPassedActivityInput = cardsPassedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowActivityContext, cardsPassedActivityInput);

        // Assert

        Assert.True(result.IsSuccess);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        [Substitute, Frozen] WorkflowActivityContext workflowActivityContext,
        CardsPassedActivity sut,
        CardsPassedActivityInput cardsPassedActivityInput)
    {
        // Arrange

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), Arg.Any<string>(), Arg.Any<ActorProxyOptions>())
            .Throws(new Exception());

        cardsPassedActivityInput = cardsPassedActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowActivityContext, cardsPassedActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
