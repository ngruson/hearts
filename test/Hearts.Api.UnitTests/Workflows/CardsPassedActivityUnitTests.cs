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

namespace Hearts.Api.UnitTests.Workflows;

public class CardsPassedActivityUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_pass_cards_completes(
        [Substitute, Frozen] WorkflowActivityContext workflowActivityContext,
        CardsPassedActivity sut,
        CardsPassedEvent cardsPassedEvent)
    {
        // Arrange

        // Act

        Result result = await sut.RunAsync(workflowActivityContext, cardsPassedEvent);

        // Assert

        Assert.True(result.IsSuccess);
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_was_thrown(
        [Substitute, Frozen] IActorProxyFactory actorProxyFactory,
        [Substitute, Frozen] WorkflowActivityContext workflowActivityContext,
        CardsPassedActivity sut,
        CardsPassedEvent cardsPassedEvent)
    {
        // Arrange

        actorProxyFactory.CreateActorProxy<IGameActor>(Arg.Any<ActorId>(), Arg.Any<string>(), Arg.Any<ActorProxyOptions>())
            .Throws(new Exception());

        // Act

        Result result = await sut.RunAsync(workflowActivityContext, cardsPassedEvent);

        // Assert

        Assert.True(result.IsError());
    }
}
