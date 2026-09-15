using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IUserExtractor
{
    IAsyncEnumerable<ElfhUser> ExtractAsync(IEnumerable<int> userIds,CancellationToken cancellationToken = default);
}