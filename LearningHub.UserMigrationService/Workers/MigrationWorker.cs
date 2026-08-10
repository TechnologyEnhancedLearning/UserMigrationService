using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Workers;

public class MigrationWorker : BackgroundService
{
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MigrationWorker(
        ILogger<MigrationWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Migration started.");

        using var scope = _scopeFactory.CreateScope();

        var pipeline = scope.ServiceProvider
            .GetRequiredService<IMigrationPipeline>();

        await pipeline.ExecuteAsync(stoppingToken);

        _logger.LogInformation("Migration completed.");
    }
}