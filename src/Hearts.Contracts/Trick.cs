namespace Hearts.Contracts;

public record Trick(Player[] Players, TrickCard[] TrickCards, Suit? Suit, Turn? Turn, bool IsCompleted, Guid? Winner);
