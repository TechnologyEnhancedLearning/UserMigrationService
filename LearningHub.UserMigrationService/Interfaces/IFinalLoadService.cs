using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IFinalLoadService
{
    Task<FinalLoadResult> ExecuteAsync(
        string procedureName,
        Guid migrationRunId,
        CancellationToken cancellationToken);
}
