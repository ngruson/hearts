using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

class StartNewRoundActivity(IActorProxyFactory actorProxyFactory) : WorkflowActivity<StartNewRoundActivityInput, Result<Contracts.Round>>
{
    public override async Task<Result<Contracts.Round>> RunAsync(WorkflowActivityContext context, StartNewRoundActivityInput input)
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

            Contracts.Round round = await actorProxy.StartNewRound();
            return round;
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
