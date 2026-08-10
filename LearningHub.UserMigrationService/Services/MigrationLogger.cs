using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;

public class MigrationLogger : IMigrationLogger
{
    private readonly IMigrationRunRepository _repository;

    public MigrationLogger(IMigrationRunRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> StartMigrationAsync(
        string migrationName,
        string startedBy,
        string appVersion)
    {
        var run = new MigrationRun
        {
            MigrationRunId = Guid.NewGuid(),
            MigrationName = migrationName,
            StartedUtc = DateTime.UtcNow,
            Status = "Running"
        };

        await _repository.InsertMigrationRunAsync(run);

        return run.MigrationRunId;
    }

    public async Task CompleteMigrationAsync(Guid migrationRunId)
    {
        await _repository.UpdateMigrationRunAsync(
            new MigrationRun
            {
                MigrationRunId = migrationRunId,
                CompletedUtc = DateTime.UtcNow,
                Status = "Completed"
            });
    }

    public async Task FailMigrationAsync(
        Guid migrationRunId,
        Exception exception)
    {
        await _repository.UpdateMigrationRunAsync(
            new MigrationRun
            {
                MigrationRunId = migrationRunId,
                CompletedUtc = DateTime.UtcNow,
                Status = "Failed"
            });
    }

    public async Task<Guid> StartStepAsync(
        Guid migrationRunId,
        string stepName)
    {
        var step = new MigrationStepRun
        {
            MigrationStepRunId = Guid.NewGuid(),
            MigrationRunId = migrationRunId,
            StepName = stepName,
            StartedUtc = DateTime.UtcNow,
            Status = "Running"
        };

        await _repository.InsertMigrationStepRunAsync(step);

        return step.MigrationStepRunId;
    }

    public async Task CompleteStepAsync(
        Guid migrationStepRunId,
        MigrationStatistics statistics)
    {
        await _repository.UpdateMigrationStepRunAsync(
            new MigrationStepRun
            {
                MigrationStepRunId = migrationStepRunId,
                CompletedUtc = DateTime.UtcNow,
                Status = "Completed"
            },
            statistics);
    }

    public async Task FailStepAsync(
        Guid migrationStepRunId,
        Exception exception)
    {
        await _repository.UpdateMigrationStepRunAsync(
            new MigrationStepRun
            {
                MigrationStepRunId = migrationStepRunId,
                CompletedUtc = DateTime.UtcNow,
                Status = "Failed"
            },
            new MigrationStatistics(),
            exception.Message);
    }
}