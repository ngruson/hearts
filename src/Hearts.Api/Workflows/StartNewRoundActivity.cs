using System.Diagnostics;
using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;

namespace Hearts.Api.Workflows;

class StartNewRoundActivity(IActorProxyFactory actorProxyFactory, Instrumentation instrumentation) : WorkflowActivity<StartNewRoundActivityInput, Result<StartNewRoundActivityOutput>>
{
    public override async Task<Result<StartNewRoundActivityOutput>> RunAsync(WorkflowActivityContext context, StartNewRoundActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.StartActivity(nameof(StartNewRoundActivity), ActivityKind.Internal, activityContext);

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
            Contracts.Game game = await actorProxy.Map(context.InstanceId);

            return new StartNewRoundActivityOutput(game, round);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
