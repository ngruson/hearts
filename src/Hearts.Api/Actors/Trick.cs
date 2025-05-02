using Hearts.Contracts;

namespace Hearts.Api.Actors;

public class Trick(Guid id, Guid roundId, Player[] players, RoundPlayer turn, Suit? suit = null)
{
    public Guid Id { get; set; } = id;
    public Guid RoundId { get; set; } = roundId;
    public Player[] Players { get; set; } = players;
    public Suit? Suit { get; set; } = suit;
    public TrickCard[] TrickCards { get; set; } = [];
    public RoundPlayer PlayerTurn { get; set; } = turn;
    public Player? Winner { get; set; }
    public bool IsCompleted { get; set; }

    public void AddCard(Card card, Guid playerId)
    {
        this.TrickCards = [.. this.TrickCards, new TrickCard(card, playerId)];

        if (this.TrickCards.Length == 1 && this.Suit is null)
        {
            this.Suit = card.Suit;
        }
    }

    internal static Trick Create(Guid roundId, Player[] players, RoundPlayer turn, Suit? suit)
    {
        Player[] orderedPlayers = [turn.Player];
        
        int currentIndex = Array.IndexOf(players, turn.Player);
        for (int i = 1; i < players.Length; i++)
        {
            int nextIndex = (currentIndex + i) % players.Length;
            orderedPlayers = [.. orderedPlayers, players[nextIndex]];
        }

        Trick trick = new(Guid.CreateVersion7(), roundId, orderedPlayers, turn, suit);

        return trick;
    }

    public Contracts.Trick Map()
    {
        return new(
            this.Id,
            this.RoundId,
            [.. this.Players],
            [.. this.TrickCards.Select(_ => _.Map())],
            this.Suit,
            this.PlayerTurn is not null ?
                new Turn(this.PlayerTurn.Player.Id, this.PlayerTurn.Player.PlayerName, this.PlayerTurn.Player.IsBot) :
                null,
            this.IsCompleted,
            this.Winner?.Id);
    }

    public void SetPlayerTurn(RoundPlayer player)
    {
        this.PlayerTurn = player;
    }

    internal void CheckCompleted()
    {
        if (this.TrickCards.Length == 4)
        {
            this.IsCompleted = true;

            Guid playerId = this.TrickCards
                .Where(_ => _.Card.Suit == this.Suit)
                .OrderByDescending(_ => _.Card.Rank)
                .First()
                .PlayerId;

            this.Winner = this.Players.SingleOrDefault(_ => _.Id == playerId);
        }
    }
}
