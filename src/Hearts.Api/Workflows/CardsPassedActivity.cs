using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;

namespace Hearts.Api.Workflows;

public class CardsPassedActivity(IActorProxyFactory actorProxyFactory) : WorkflowActivity<CardsPassedEvent, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, CardsPassedEvent input)
    {
        try
        {
            ActorProxyOptions actorProxyOptions = new()
            {
                UseJsonSerialization = true,
                JsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            };

            ActorId actorId = new(input.GameId.ToString());
            IGameActor actorProxy = actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), actorProxyOptions);

            await actorProxy.PassCards(input.CardsPassed);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
