using Dapr.Actors.Runtime;

namespace Hearts.Api.Actors;

internal class PlayerActor(ActorHost host) : Actor(host), IPlayerActor
{
}
