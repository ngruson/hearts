using Hearts.Api.Queries;
using Marten;

namespace Hearts.Api.Services;

public class GameMonitorService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            Endpoints.GameResponse registeringGames =
                await GetGamesHandler.Handle(
                    scope.ServiceProvider.GetRequiredService<IQuerySession>(),
                    true);

            if (registeringGames is null || registeringGames.Games.Length == 0)
            {
                GameHub hub = scope.ServiceProvider.GetRequiredService<GameHub>();
                await hub.CreateNewGame();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
