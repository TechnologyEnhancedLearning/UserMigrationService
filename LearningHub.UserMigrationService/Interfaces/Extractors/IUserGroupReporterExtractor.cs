using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IUserGroupReporterExtractor
{
    IAsyncEnumerable<ElfhUserGroupReporter> ExtractAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default);
}