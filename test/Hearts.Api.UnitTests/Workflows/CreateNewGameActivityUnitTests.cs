using System.Diagnostics;
using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.Workflows;
using Hearts.Contracts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Hearts.Api.UnitTests.Workflows;

public class CreateNewGameActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_game_was_created(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        [Substitute, Frozen] IGameActor gameActor,
        CreateNewGameActivityInput createNewGameActivityInput,
        Game game,
        CreateNewGameActivity sut)
    {
        // Arrange

        gameActor.Map(workflowContext.InstanceId).Returns(game);

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Returns(gameActor);

        createNewGameActivityInput = createNewGameActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result<Game> result = await sut.RunAsync(workflowContext, createNewGameActivityInput);

        // Assert

        Assert.True(result.IsSuccess);
        Assert.Equal(game, result.Value);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        CreateNewGameActivityInput createNewGameActivityInput,
        CreateNewGameActivity sut)
    {
        // Arrange

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Throws<Exception>();

        createNewGameActivityInput = createNewGameActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result<Game> result = await sut.RunAsync(workflowContext, createNewGameActivityInput);

        // Assert

        Assert.True(result.IsError());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown_given_no_activity(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        CreateNewGameActivityInput createNewGameActivityInput,
        CreateNewGameActivity sut)
    {
        // Arrange

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Throws<Exception>();

        createNewGameActivityInput = createNewGameActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result<Game> result = await sut.RunAsync(workflowContext, createNewGameActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
