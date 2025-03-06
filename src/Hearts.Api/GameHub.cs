using System.Diagnostics;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;
using Hearts.Api.Workflows;
using Hearts.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Hearts.Api;

public class GameHub(DaprWorkflowClient daprWorkflowClient, Instrumentation instrumentation) : Hub<IGameClient>
{
    public new virtual IHubCallerClients<IGameClient> Clients
    {
        get
        {            
            return base.Clients;
        }
        set => base.Clients = value;
    }

    public async Task CreatePlayer(string name)
    {
        if (this.Clients.Caller is null)
        {
            return;            
        }

        using Activity? activity = instrumentation.ActivitySource.StartActivity(nameof(CreatePlayer));

        ActorId actorId = new(Guid.CreateVersion7().ToString());
        IPlayerActor playerActor = ActorProxy.Create<IPlayerActor>(actorId, nameof(PlayerActor));
        Player player = new(Guid.Parse(actorId.GetId()), name);

        await this.Clients.Caller.PlayerCreated(player);
    }

    public async Task CreateNewGame(Player player)
    {        
        using Activity? activity = instrumentation.ActivitySource.StartActivity(nameof(CreateNewGame));

        if (activity is null)
            return;

        GameWorkflowInput input = new(activity.TraceId.ToString(), activity.SpanId.ToString(), player);
        await daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(GameWorkflow), null, input);
    }

    public async Task PassCards(Guid gameId, string workflowInstanceId, PassCard[] passCards)
    {
        using Activity? activity = instrumentation.ActivitySource.StartActivity(nameof(PassCards));

        await daprWorkflowClient.RaiseEventAsync(workflowInstanceId,
            GameWorkflowEvents.CardsPassed,
            new CardsPassedEvent(gameId, passCards));
    }
}
