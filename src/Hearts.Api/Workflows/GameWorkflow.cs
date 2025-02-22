using Ardalis.Result;
using Dapr.Workflow;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class GameWorkflow : Workflow<GameWorkflowInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowContext context, GameWorkflowInput input)
    {
        try
        {
            Result<Game> createNewGameResult = await context.CallActivityAsync<Result<Game>>(
                nameof(CreateNewGameActivity),
                new CreateNewGameActivityInput(input.Player));

            if (createNewGameResult.IsSuccess)
            {
                await context.CallActivityAsync(
                    nameof(NotifyGameUpdatedActivity),
                    new NotifyGameUpdatedActivityInput(createNewGameResult.Value));
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
                    new AddBotPlayerActivityInput(game.Id));

                if (addBotPlayerResult.IsSuccess)
                {
                    await context.CallActivityAsync(
                        nameof(NotifyGameUpdatedActivity),
                        new NotifyGameUpdatedActivityInput(addBotPlayerResult.Value));

                    game = addBotPlayerResult.Value;
                }
                else
                {
                    return Result.Error("Failed to add bot player");
                }
            }

            Result<Contracts.Round> startNewRoundActivityResult = await context.CallActivityAsync<Result<Contracts.Round>>(
                nameof(StartNewRoundActivity),
                new StartNewRoundActivityInput(game.Id));

            if (startNewRoundActivityResult.IsSuccess)
            {
                await context.CallActivityAsync(
                    nameof(NotifyRoundStartedActivity),
                    new NotifyRoundStartedActivityInput(startNewRoundActivityResult.Value));
            }
            else
            {
                return Result.Error("Failed to start new round");
            }

            // Start the game

            // Manage tricks
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }

        return Result.Success();
    }
}
