using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IUserAdminLocationExtractor
{
    IAsyncEnumerable<ElfhUserAdminLocation> ExtractAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default);
}