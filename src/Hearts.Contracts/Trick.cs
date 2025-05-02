namespace Hearts.Contracts;

public record Trick(Guid Id, Guid RoundId, Player[] Players, TrickCard[] TrickCards, Suit? Suit, Turn? Turn, bool IsCompleted, Guid? Winner);
