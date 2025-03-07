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

internal class AddBotPlayerActivity(IActorProxyFactory actorProxyFactory, Instrumentation instrumentation) : WorkflowActivity<AddBotPlayerActivityInput, Result<Game>>
{
    public override async Task<Result<Game>> RunAsync(WorkflowActivityContext context, AddBotPlayerActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.StartActivity(nameof(AddBotPlayerActivity), ActivityKind.Internal, activityContext);

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

            Game game = await actorProxy.Map(context.InstanceId);

            return game;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            return Result.Error(ex.Message);
        }
    }
}
