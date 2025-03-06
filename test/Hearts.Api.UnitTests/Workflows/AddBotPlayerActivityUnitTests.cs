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

namespace Hearts.Api.UnitTests.Workflows;

public class AddBotPlayerActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_bot_player_was_added(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        [Substitute, Frozen] IGameActor gameActor,
        AddBotPlayerActivityInput addBotPlayerActivityInput,
        Game game,
        AddBotPlayerActivity sut)
    {
        // Arrange

        gameActor.Map(workflowContext.InstanceId).Returns(game);

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Returns(gameActor);

        addBotPlayerActivityInput = addBotPlayerActivityInput with
            {
                TraceId = ActivityTraceId.CreateRandom().ToString(),
                SpanId = ActivitySpanId.CreateRandom().ToString()
            };

        // Act

        Result<Game> result = await sut.RunAsync(workflowContext, addBotPlayerActivityInput);

        // Assert

        Assert.True(result.IsSuccess);
        Assert.Equal(game, result.Value);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        AddBotPlayerActivityInput addBotPlayerActivityInput,
        AddBotPlayerActivity sut)
    {
        // Arrange

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Throws<Exception>();

        addBotPlayerActivityInput = addBotPlayerActivityInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result<Game> result = await sut.RunAsync(workflowContext, addBotPlayerActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
