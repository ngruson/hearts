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

public class StartNewRoundActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_client_was_notified(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        [Substitute, Frozen] IGameActor gameActor,
        StartNewRoundActivityInput startNewRoundActivityInput,
        StartNewRoundActivity sut,
        Contracts.Round round)
    {
        // Arrange

        gameActor.StartNewRound().Returns(round);

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Returns(gameActor);

        // Act

        Result<Contracts.Round> result = await sut.RunAsync(workflowContext, startNewRoundActivityInput);

        // Assert

        Assert.True(result.IsSuccess);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] WorkflowActivityContext workflowContext,
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        StartNewRoundActivityInput startNewRoundActivityInput,
        StartNewRoundActivity sut)
    {
        // Arrange

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), nameof(GameActor), Arg.Any<ActorProxyOptions>())
            .Throws<Exception>();

        // Act

        Result<Contracts.Round> result = await sut.RunAsync(workflowContext, startNewRoundActivityInput);

        // Assert

        Assert.True(result.IsError());
    }
}
