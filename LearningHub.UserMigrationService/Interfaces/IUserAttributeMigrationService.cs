using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IUserAttributeMigrationService
{
    Task<MigrationStatistics> ExtractAndStageUserAttributesAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken);
}