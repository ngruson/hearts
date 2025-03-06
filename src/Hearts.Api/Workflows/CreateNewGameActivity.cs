using System.Diagnostics;
using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;
using Hearts.Contracts;

namespace Hearts.Api.Workflows;

internal class CreateNewGameActivity(IActorProxyFactory actorProxyFactory, Instrumentation instrumentation) : WorkflowActivity<CreateNewGameActivityInput, Result<Game>>
{
    public override async Task<Result<Game>> RunAsync(WorkflowActivityContext context, CreateNewGameActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.ActivitySource.StartActivity(nameof(CreateNewGameActivity), ActivityKind.Internal, activityContext);

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

            Guid guid = Guid.CreateVersion7();
            ActorId actorId = new(guid.ToString());
            IGameActor actorProxy = actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), actorProxyOptions);

            await actorProxy.AddPlayer(input.Player);

            return await actorProxy.Map(context.InstanceId);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            return Result.Error(ex.Message);
        }
    }
}
