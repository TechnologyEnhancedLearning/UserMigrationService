using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IUserEmploymentExtractor
{
    IAsyncEnumerable<ElfhUserEmployment> ExtractAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken = default);
}