using System.Diagnostics;
using Ardalis.Result;
using Dapr.Workflow;
using Hearts.Api.OpenTelemetry;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class GameWorkflow : Workflow<GameWorkflowInput, Result>
{
    public Instrumentation Instrumentation { get; set; } = new();

    public override async Task<Result> RunAsync(WorkflowContext context, GameWorkflowInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = this.Instrumentation.StartActivity(nameof(GameWorkflow), ActivityKind.Internal, activityContext);

        if (activity is null)
        {
            return Result.Error("Failed to start activity");
        }

        try
        {
            Result<Game> createNewGameResult = await context.CallActivityAsync<Result<Game>>(
                nameof(CreateNewGameActivity),
                new CreateNewGameActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), input.Player));

            if (createNewGameResult.IsSuccess)
            {
                await context.CallActivityAsync(
                    nameof(NotifyGameUpdatedActivity),
                    new NotifyGameUpdatedActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), createNewGameResult.Value));
            }
            else
            {
                return Result.Error("Failed to create new game");
            }

            Game game = createNewGameResult.Value;

            for (int i = game.Players!.Length; i < 4; i++)
            {
                Result<Game> addBotPlayerResult = await context.CallActivityAsync<Result<Game>>(
                    nameof(AddBotPlayerActivity),
                    new AddBotPlayerActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), game.Id));

                if (addBotPlayerResult.IsSuccess)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyGameUpdatedActivity),
                        new NotifyGameUpdatedActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), addBotPlayerResult.Value));

                    game = addBotPlayerResult.Value;
                }
                else
                {
                    return Result.Error("Failed to add bot player");
                }
            }

            Result<StartNewRoundActivityOutput> startNewRoundActivityResult = await context.CallActivityAsync<Result<StartNewRoundActivityOutput>>(
                nameof(StartNewRoundActivity),
                new StartNewRoundActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), game.Id));

            if (startNewRoundActivityResult.IsSuccess)
            {
                await context.CallActivityAsync(
                    nameof(NotifyRoundStartedActivity),
                    new NotifyRoundStartedActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), startNewRoundActivityResult.Value.Round));

                await context.CallActivityAsync(
                    nameof(NotifyGameUpdatedActivity),
                    new NotifyGameUpdatedActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), startNewRoundActivityResult.Value.Game));
            }
            else
            {
                return Result.Error("Failed to start new round");
            }

            CardsPassedEvent cardsPassedEvent = await context.WaitForExternalEventAsync<CardsPassedEvent>(GameWorkflowEvents.CardsPassed);
            await context.CallActivityAsync(
                nameof(CardsPassedActivity),
                new CardsPassedActivityInput(activity.TraceId.ToString(), activity.SpanId.ToString(), cardsPassedEvent));

            // Start the game

            // Manage tricks
        }
        catch (Exception ex)
        {
            activity.AddException(ex);
            return Result.Error(ex.Message);
        }

        return Result.Success();
    }
}
