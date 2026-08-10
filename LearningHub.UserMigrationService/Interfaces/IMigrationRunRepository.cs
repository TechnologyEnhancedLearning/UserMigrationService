using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IMigrationRunRepository
{
    Task InsertMigrationRunAsync(MigrationRun migrationRun);

    Task UpdateMigrationRunAsync(MigrationRun migrationRun);

    Task InsertMigrationStepRunAsync(MigrationStepRun stepRun);

    Task UpdateMigrationStepRunAsync(MigrationStepRun stepRun,MigrationStatistics statistics,string? errorMessage = null);

    Task InsertMigrationLogAsync(MigrationLog log);
}