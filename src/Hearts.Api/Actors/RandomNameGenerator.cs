namespace Hearts.Api.Actors;

public static class RandomNameGenerator
{
    private static readonly string[] Names = { "Alice", "Bob", "Charlie", "Diana" };
    private static readonly Random Random = new();

    public static string GenerateRandomName()
    {
        int index = Random.Next(Names.Length);
        return Names[index];
    }
}
