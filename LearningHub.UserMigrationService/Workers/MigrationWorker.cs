using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Workers;

public class MigrationWorker : BackgroundService
{
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IHostApplicationLifetime  _applicationLifetime;
    public MigrationWorker(
        ILogger<MigrationWorker> logger,
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _applicationLifetime =applicationLifetime;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("***** MIGRATION WORKER STARTED *****");

        try
        {
            using var scope = _scopeFactory.CreateScope();

            var pipeline = scope.ServiceProvider
                    .GetRequiredService<IMigrationPipeline>();

            await pipeline.ExecuteAsync(stoppingToken);

            _logger.LogInformation("***** MIGRATION WORKER COMPLETED *****");

            await Task.CompletedTask;
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Migration worker cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Migration worker failed.");

            throw;
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}