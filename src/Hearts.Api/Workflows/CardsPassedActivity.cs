using System.Diagnostics;
using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Workflow;
using Hearts.Api.Actors;
using Hearts.Api.OpenTelemetry;

namespace Hearts.Api.Workflows;

public class CardsPassedActivity(IActorProxyFactory actorProxyFactory, Instrumentation instrumentation) : WorkflowActivity<CardsPassedActivityInput, Result>
{
    public override async Task<Result> RunAsync(WorkflowActivityContext context, CardsPassedActivityInput input)
    {
        ActivityContext activityContext = new(ActivityTraceId.CreateFromString(input.TraceId), ActivitySpanId.CreateFromString(input.SpanId), ActivityTraceFlags.Recorded);
        using Activity? activity = instrumentation.StartActivity(nameof(CardsPassedActivity), ActivityKind.Internal, activityContext);

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

            ActorId actorId = new(input.CardsPassedEvent.GameId.ToString());
            IGameActor actorProxy = actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), actorProxyOptions);

            await actorProxy.PassCards(input.CardsPassedEvent.CardsPassed);

            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            return Result.Error(ex.Message);
        }
    }
}
