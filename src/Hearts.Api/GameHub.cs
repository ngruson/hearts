using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.Workflows;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Hearts.Api;

public class GameHub(DaprWorkflowClient daprWorkflowClient) : Hub<IGameClient>
{
    public async Task CreatePlayer(string name)
    {
        if (this.Clients.Caller is null)
        {
            return;            
        }

        ActorId actorId = new(Guid.CreateVersion7().ToString());
        IPlayerActor playerActor = ActorProxy.Create<IPlayerActor>(actorId, nameof(PlayerActor));
        Player player = new(Guid.Parse(actorId.GetId()), name);

        await this.Clients.Caller.PlayerCreated(player);
    }

    public async Task CreateNewGame(Player player)
    {
        GameWorkflowInput input = new(player);
        await daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(GameWorkflow), null, input);
    }

    public async Task PassCards(Guid gameId, string workflowInstanceId, PassCard[] passCards)
    {
        await daprWorkflowClient.RaiseEventAsync(workflowInstanceId,
            GameWorkflowEvents.CardsPassed,
            new CardsPassedEvent(gameId, passCards));
    }
}
