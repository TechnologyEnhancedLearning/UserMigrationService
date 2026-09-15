using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IProfessionalBodyExtractor
{
    IAsyncEnumerable<ElfhProfessionalBody> ExtractAsync(
        CancellationToken cancellationToken = default);
}