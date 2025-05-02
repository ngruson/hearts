using System.Diagnostics;
using System.Text.Json;
using Ardalis.Result;
using Dapr.Actors;
using Dapr.Actors.Client;
using Hearts.Api.Actors;
using Hearts.Api.Eventing.Events;
using Hearts.Api.Eventing.Projections;
using Hearts.Api.OpenTelemetry;
using Hearts.Contracts;
using Hearts.Contracts.Events;
using Marten;
using Microsoft.AspNetCore.SignalR;

namespace Hearts.Api;

public class GameHub : Hub<IGameClient>
{
    private readonly IActorProxyFactory actorProxyFactory;
    private readonly ActorProxyOptions actorProxyOptions;
    private readonly Instrumentation instrumentation;
    private readonly IDocumentStore documentStore;

    public GameHub(IActorProxyFactory actorProxyFactory, IDocumentStore documentStore, Instrumentation instrumentation)
    {
        this.actorProxyFactory = actorProxyFactory;
        this.actorProxyOptions = new()
        {
            UseJsonSerialization = true,
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        };

        this.documentStore = documentStore;
        this.instrumentation = instrumentation;
    }

    public new virtual IHubCallerClients<IGameClient> Clients
    {
        get
        {            
            return base.Clients;
        }
        set => base.Clients = value;
    }

    public async Task CreateNewGame(Player player)
    {
        using Activity? activity = this.instrumentation.StartActivity(nameof(CreateNewGame));
        if (activity is null)
        {
            return;
        }

        try
        {
            ActorId actorId = new(Guid.CreateVersion7().ToString());
            IGameActor gameActor = this.actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), this.actorProxyOptions);

            GameCreatedEvent gameCreatedEvent = new(Guid.Parse(actorId.GetId()));
            await using IDocumentSession session = this.documentStore.LightweightSession();
            session.Events.StartStream<GameProjection>(gameCreatedEvent.GameId, gameCreatedEvent);
            await session.SaveChangesAsync();

            await gameActor.AddPlayer(player);
            Game game = await gameActor.Map();

            await this.NotifyGameUpdated(activity, game);

            await this.AddBotPlayers(gameActor, activity);

            await this.StartNewRound(gameActor, activity);
        }
        catch (Exception ex)
        {
            activity.AddException(ex);
        }
    }

    public async Task CreatePlayer(string name)
    {
        if (this.Clients.Caller is null)
        {
            return;            
        }

        using Activity? activity = this.instrumentation.StartActivity(nameof(CreatePlayer));
        if (activity is null)
        {
            return;
        }

        try
        {
            ActorId actorId = new(Guid.CreateVersion7().ToString());
            IPlayerActor playerActor = this.actorProxyFactory.CreateActorProxy<IPlayerActor>(actorId, nameof(PlayerActor));
            Player player = new(Guid.Parse(actorId.GetId()), name);

            await this.Clients.Caller.PlayerCreated(player);
        }
        catch (Exception ex)
        {
            activity.AddException(ex);
        }
    }
    public async Task PassCards(Guid gameId, PassCard[] passCards)
    {
        using Activity? activity = this.instrumentation.StartActivity(nameof(PassCards));
        if (activity is null)
        {
            return;
        }

        try
        {
            ActorId actorId = new(gameId.ToString());
            IGameActor gameActor = this.actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), this.actorProxyOptions);

            await gameActor.PassCards(passCards);
            Game game = await gameActor.Map();
            await this.NotifyGameUpdated(activity, game);

            await this.StartTrick(gameActor);
        }
        catch (Exception ex)
        {
            activity.AddException(ex);
        }
    }

    public async Task PlayCard(Guid gameId, Guid playerId, Card card)
    {
        using Activity? activity = this.instrumentation.StartActivity(nameof(PlayCard));
        if (activity is null)
        {
            return;
        }

        try
        {
            ActorId actorId = new(gameId.ToString());
            IGameActor gameActor = this.actorProxyFactory.CreateActorProxy<IGameActor>(actorId, nameof(GameActor), this.actorProxyOptions);

            Result validationResult = await gameActor.ValidateCard(card);

            if (!validationResult.IsSuccess)
            {
                await this.NotifyInvalidCardPlayed(activity, card, validationResult.ValidationErrors.FirstOrDefault()?.ErrorMessage);
                return;
            }

            await gameActor.PlayCard(playerId, card);
            await gameActor.PlayBots();

            Game game = await gameActor.Map();
            Contracts.Trick? trick = game.CurrentRound?.Tricks.LastOrDefault();
            await this.NotifyGameUpdated(activity, game);

            if (game.IsCompleted is false)
            {
                if (game.CurrentRound?.IsCompleted == true)
                {
                    await gameActor.StartRound();
                    game = await gameActor.Map();

                    if (game.PassingDirection == PassingDirection.None)
                    {
                        await gameActor.StartTrick();
                        await gameActor.PlayBots();
                        game = await gameActor.Map();
                    }
                    await this.NotifyGameUpdated(activity, game);
                }
                else if (trick?.IsCompleted == true)
                {
                    game = await gameActor.Map();
                    await this.NotifyGameUpdated(activity, game);

                    await Task.Delay(1000);
                    await this.StartTrick(gameActor);

                    game = await gameActor.Map();
                    await this.NotifyGameUpdated(activity, game);
                }
            }
        }
        catch (Exception ex)
        {
            activity.AddException(ex);
        }
    }

    private async Task AddBotPlayers(IGameActor gameActor, Activity activity)
    {
        for (int i = (await gameActor.Map()).Players.Length; i < 4; i++)
        {
            await gameActor.AddBotPlayer();
            await this.NotifyGameUpdated(activity, await gameActor.Map());
        }
    }

    private async Task NotifyGameUpdated(Activity parentActivity, Game game)
    {
        ActivityContext activityContext = new(parentActivity.TraceId, parentActivity.SpanId, ActivityTraceFlags.Recorded);
        using Activity? activity = this.instrumentation.StartActivity(nameof(NotifyGameUpdated), ActivityKind.Internal, activityContext);

        try
        {
            await this.Clients.Caller.GameUpdated(game);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
    }

    private async Task NotifyInvalidCardPlayed(Activity parentActivity, Card card, string? message)
    {
        ActivityContext activityContext = new(parentActivity.TraceId, parentActivity.SpanId, ActivityTraceFlags.Recorded);
        using Activity? activity = this.instrumentation.StartActivity(nameof(NotifyInvalidCardPlayed), ActivityKind.Internal, activityContext);

        try
        {
            await this.Clients.Caller.InvalidCardPlayed(new InvalidCardPlayedEvent(card, message));
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
    }

    private async Task StartNewRound(IGameActor gameActor, Activity parentActivity)
    {
        ActivityContext activityContext = new(parentActivity.TraceId, parentActivity.SpanId, ActivityTraceFlags.Recorded);
        using Activity? activity = this.instrumentation.StartActivity(nameof(StartNewRound), ActivityKind.Internal, activityContext);
        if (activity is null)
        {
            return;
        }

        try
        {
            await gameActor.StartRound();
            Game game = await gameActor.Map();
            await this.NotifyGameUpdated(parentActivity, game);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
    }
    private async Task StartTrick(IGameActor gameActor)
    {
        using Activity? activity = this.instrumentation.StartActivity(nameof(StartTrick));
        if (activity is null)
        {
            return;
        }

        await gameActor.StartTrick();
        await gameActor.PlayBots();

        Game game = await gameActor.Map();
        await this.NotifyGameUpdated(activity, game);
    }
}
