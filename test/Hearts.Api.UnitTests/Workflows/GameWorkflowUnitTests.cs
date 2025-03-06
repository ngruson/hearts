using System.Diagnostics;
using Ardalis.Result;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Dapr.Workflow;
using Hearts.Api.OpenTelemetry;
using Hearts.Api.Workflows;
using Hearts.Contracts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Hearts.Api.UnitTests.Workflows;

public class GameWorkflowUnitTests
{
    [Theory, AutoNSubstituteData]
    internal async Task return_success_when_workflow_runs_ok(
        [Substitute, Frozen] Instrumentation instrumentation,
        [Substitute, Frozen] WorkflowContext workflowContext,
        Activity activity,
        GameWorkflowInput gameWorkflowInput,
        GameWorkflow sut,
        Round round,
        CardsPassedEvent cardsPassedEvent)
    {
        // Arrange

        Game newGame = new(Guid.NewGuid(), workflowContext.InstanceId, [gameWorkflowInput.Player]);

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>())
        .Returns(Result.Success(newGame));

        for (int i = 0; i < 3; i++)
        {
            newGame = newGame with { Players = [.. newGame.Players!, new Player(Guid.NewGuid(), "Bot")] };

            workflowContext.CallActivityAsync<Result<Game>>(
                nameof(AddBotPlayerActivity),
                Arg.Any<AddBotPlayerActivityInput>())
            .Returns(Result.Success(newGame));
        }

        workflowContext.CallActivityAsync<Result<StartNewRoundActivityOutput>>(
            nameof(StartNewRoundActivity),
            Arg.Any<StartNewRoundActivityInput>())
        .Returns(Result.Success(new StartNewRoundActivityOutput(newGame, round)));

        workflowContext.WaitForExternalEventAsync<CardsPassedEvent>(GameWorkflowEvents.CardsPassed)
            .Returns(cardsPassedEvent);

        instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, Arg.Any<ActivityContext>())
           .Returns(activity);

        sut.Instrumentation = instrumentation;

        gameWorkflowInput = gameWorkflowInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, gameWorkflowInput);

        // Assert

        Assert.True(result.IsSuccess);

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>());

        await workflowContext.Received(3).CallActivityAsync<Result<Game>>(
            nameof(AddBotPlayerActivity),
            Arg.Any<AddBotPlayerActivityInput>());

        await workflowContext.Received(5).CallActivityAsync(
            nameof(NotifyGameUpdatedActivity),
            Arg.Any<NotifyGameUpdatedActivityInput>());

        await workflowContext.CallActivityAsync<Result<StartNewRoundActivityOutput>>(
            nameof(StartNewRoundActivity),
            Arg.Any<StartNewRoundActivityInput>());

        await workflowContext.Received().CallActivityAsync(
            nameof(NotifyRoundStartedActivity),
            Arg.Any<NotifyRoundStartedActivityInput>());

        await workflowContext.Received().WaitForExternalEventAsync<CardsPassedEvent>(
            nameof(GameWorkflowEvents.CardsPassed));

        await workflowContext.Received().CallActivityAsync(
            nameof(CardsPassedActivity),
            Arg.Any<CardsPassedActivityInput>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_game_could_not_be_created(
        [Substitute, Frozen] Instrumentation instrumentation,
        [Substitute, Frozen] WorkflowContext workflowContext,
        Activity activity,
        GameWorkflowInput gameWorkflowInput,
        GameWorkflow sut)
    {
        // Arrange

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>())
        .Returns(Result.Error("Failed to create new game"));

        instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, Arg.Any<ActivityContext>())
            .Returns(activity);

        sut.Instrumentation = instrumentation;

        gameWorkflowInput = gameWorkflowInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, gameWorkflowInput);

        // Assert

        Assert.True(result.IsError());

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>());

        await workflowContext.DidNotReceive().CallActivityAsync(
            nameof(NotifyGameUpdatedActivity),
            Arg.Any<NotifyGameUpdatedActivityInput>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_bot_player_could_not_be_added(
        [Substitute, Frozen] Instrumentation instrumentation,
        [Substitute, Frozen] WorkflowContext workflowContext,
        Activity activity,
        GameWorkflowInput gameWorkflowInput,
        GameWorkflow sut)
    {
        // Arrange

        Game newGame = new(Guid.NewGuid(), workflowContext.InstanceId, [gameWorkflowInput.Player]);

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>())
        .Returns(Result.Success(newGame));

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(AddBotPlayerActivity),
            Arg.Any<AddBotPlayerActivityInput>())
        .Returns(Result.Error("Failed to add bot player"));

        instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, Arg.Any<ActivityContext>())
            .Returns(activity);

        sut.Instrumentation = instrumentation;

        gameWorkflowInput = gameWorkflowInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, gameWorkflowInput);

        // Assert

        Assert.True(result.IsError());

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>());

        await workflowContext.Received().CallActivityAsync(
            nameof(NotifyGameUpdatedActivity),
            Arg.Any<NotifyGameUpdatedActivityInput>());

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(AddBotPlayerActivity),
            Arg.Any<AddBotPlayerActivityInput>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_round_could_not_be_started(
        [Substitute, Frozen] Instrumentation instrumentation,
        [Substitute, Frozen] WorkflowContext workflowContext,
        Activity activity,
        GameWorkflowInput gameWorkflowInput,
        GameWorkflow sut)
    {
        // Arrange

        Game newGame = new(Guid.NewGuid(), workflowContext.InstanceId, [gameWorkflowInput.Player]);

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>())
        .Returns(Result.Success(newGame));

        for (int i = 0; i < 3; i++)
        {
            newGame = newGame with { Players = [.. newGame.Players!, new Player(Guid.NewGuid(), "Bot")] };

            workflowContext.CallActivityAsync<Result<Game>>(
                nameof(AddBotPlayerActivity),
                Arg.Any<AddBotPlayerActivityInput>())
            .Returns(Result.Success(newGame));
        }

        workflowContext.CallActivityAsync<Result<Round>>(
            nameof(StartNewRoundActivity),
            Arg.Any<StartNewRoundActivityInput>())
        .Returns(Result.Error("Failed to start new round"));

        instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, Arg.Any<ActivityContext>())
            .Returns(activity);

        sut.Instrumentation = instrumentation;

        gameWorkflowInput = gameWorkflowInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, gameWorkflowInput);

        // Assert

        Assert.True(result.IsError());

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>());

        await workflowContext.Received(3).CallActivityAsync<Result<Game>>(
            nameof(AddBotPlayerActivity),
            Arg.Any<AddBotPlayerActivityInput>());

        await workflowContext.Received(4).CallActivityAsync(
            nameof(NotifyGameUpdatedActivity),
            Arg.Any<NotifyGameUpdatedActivityInput>());

        await workflowContext.CallActivityAsync<Result<Round>>(
            nameof(StartNewRoundActivity),
            Arg.Any<StartNewRoundActivityInput>());

        await workflowContext.DidNotReceive().CallActivityAsync(
            nameof(NotifyRoundStartedActivity),
            Arg.Any<NotifyRoundStartedActivityInput>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task return_error_when_exception_is_thrown(
        [Substitute, Frozen] Instrumentation instrumentation,
        [Substitute, Frozen] WorkflowContext workflowContext,
        Activity activity,
        GameWorkflowInput gameWorkflowInput,
        GameWorkflow sut)
    {
        // Arrange

        workflowContext.CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>())
        .ThrowsAsync<Exception>();

        instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, Arg.Any<ActivityContext>())
            .Returns(activity);

        sut.Instrumentation = instrumentation;

        gameWorkflowInput = gameWorkflowInput with
        {
            TraceId = ActivityTraceId.CreateRandom().ToString(),
            SpanId = ActivitySpanId.CreateRandom().ToString()
        };

        // Act

        Result result = await sut.RunAsync(workflowContext, gameWorkflowInput);

        // Assert

        Assert.True(result.IsError());

        await workflowContext.Received().CallActivityAsync<Result<Game>>(
            nameof(CreateNewGameActivity),
            Arg.Any<CreateNewGameActivityInput>());
    }
}
