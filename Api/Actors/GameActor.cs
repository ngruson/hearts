using Dapr.Actors.Runtime;
using Microsoft.AspNetCore.SignalR;

namespace Api.Actors;

public class GameActor(ActorHost host, IHubContext<GameHub> hubContext) : Actor(host)
{
    public async Task NotifyPlayers(string message)
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", "GameActor", message);
    }
}