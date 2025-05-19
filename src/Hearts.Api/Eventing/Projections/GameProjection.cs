using Hearts.Api.Eventing.Events;
using Marten.Events.Aggregation;

namespace Hearts.Api.Eventing.Projections;

public class GameProjection : SingleStreamProjection<Entities.Game>
{
    public static void Apply(GameCompletedEvent gameCompletedEvent, Entities.Game game)
    {
        game.State = Contracts.GameState.Completed;
    }

    public static void Apply(GameCreatedEvent gameCreatedEvent, Entities.Game game)
    {
        game.Id = gameCreatedEvent.GameId;
        game.State = Contracts.GameState.Registering;
    }

    public static void Apply(PlayerJoinedEvent playerJoinedEvent, Entities.Game game)
    {
        game.Players = [.. game.Players, playerJoinedEvent.Player];
    }

    public static void Apply(RoundStartedEvent roundStartedEvent, Entities.Game game)
    {
        game.Rounds = [.. game.Rounds,
            new Entities.Round(roundStartedEvent.Round.Id, roundStartedEvent.Round.GameId)];
    }

    public static void Apply(TrickStartedEvent trickStartedEvent, Entities.Game game)
    {
        Entities.Round? round = game.Rounds.FirstOrDefault(r => r.Id == trickStartedEvent.RoundId);
        if (round == null) return;
        round.Tricks = [.. round.Tricks, new Entities.Trick(trickStartedEvent.Id)];
    }

    public static void Apply(CardPlayedEvent cardPlayedEvent, Entities.Game game)
    {
        Entities.Round? round = game.Rounds.FirstOrDefault(r => r.Id == cardPlayedEvent.RoundId);
        if (round == null) return;

        Entities.Trick? trick = round.Tricks.FirstOrDefault(t => t.Id == cardPlayedEvent.TrickId);
        if (trick == null) return;

        trick.Cards = [.. trick.Cards, cardPlayedEvent.TrickCard];
    }
}
