using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class CreateNewGameActivity(IActorProxyFactory actorProxyFactory) : WorkflowActivity<CreateNewGameActivityInput, Result<Game>>
{
    public override async Task<Result<Game>> RunAsync(WorkflowActivityContext context, CreateNewGameActivityInput input)
    {
        try
        {
            ActorProxyOptions actorProxyOptions = new()
            {
                UseJsonSerialization = true,
                JsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                //{
                //    Converters = { new PlayerCardsJsonConverter() }
                //}
            };

            Guid guid = Guid.CreateVersion7();
            ActorId actorId = new(guid.ToString());
            IGameActor actorProxy = actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), actorProxyOptions);

            await actorProxy.AddPlayer(input.Player);

            Game game = await actorProxy.Map(context.InstanceId);


                //InvokeMethodAsync(nameof(GameActor.AddPlayer), input.Player);

            //Game game = await actorInvoker.InvokeMethodAsync<Game>(nameof(GameActor.Map));

            return game;
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
