using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class AddBotPlayerActivity(IActorProxyFactory actorProxyFactory) : WorkflowActivity<AddBotPlayerActivityInput, Result<Game>>
{
    public override async Task<Result<Game>> RunAsync(WorkflowActivityContext context, AddBotPlayerActivityInput input)
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

            await actorProxy.AddBotPlayer();

            Game game = await actorProxy.Map();

            return game;
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
