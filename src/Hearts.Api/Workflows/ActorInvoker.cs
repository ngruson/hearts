using Dapr.Actors;
using Dapr.Actors.Client;

namespace Hearts.Api.Workflows;

public class ActorInvoker
{
    private ActorProxy? actorProxy;

    public virtual Task InvokeMethodAsync(string method, CancellationToken cancellationToken = default)
    {
        return this.actorProxy!.InvokeMethodAsync(method, cancellationToken);
    }

    public virtual async Task<T> InvokeMethodAsync<T>(string methodName)
    {
        T result = await this.actorProxy!.InvokeMethodAsync<T>(methodName);
        return result;
    }

    public virtual Task InvokeMethodAsync<TRequest>(string method, TRequest data, CancellationToken cancellationToken = default)
    {
        return this.actorProxy!.InvokeMethodAsync(method, data, cancellationToken);
    }

    internal void CreateActorProxy(ActorId actorId, string actorType, ActorProxyOptions options)
    {
        this.actorProxy = ActorProxy.Create(
            actorId,
            actorType,
            options
        );
    }
}
