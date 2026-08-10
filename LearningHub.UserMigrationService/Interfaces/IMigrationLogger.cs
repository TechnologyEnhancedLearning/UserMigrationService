using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IMigrationLogger
{
    Task<Guid> StartMigrationAsync(
        string migrationName,
        string startedBy,
        string appVersion);

    Task CompleteMigrationAsync(
        Guid migrationRunId);

    Task FailMigrationAsync(
        Guid migrationRunId,
        Exception exception);

    Task<Guid> StartStepAsync(
        Guid migrationRunId,
        string stepName);

    Task CompleteStepAsync(
        Guid migrationStepRunId,
        MigrationStatistics statistics);

    Task FailStepAsync(
        Guid migrationStepRunId,
        Exception exception);
}