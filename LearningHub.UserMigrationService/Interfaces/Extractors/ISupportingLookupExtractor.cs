using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface ISupportingLookupExtractor
{
    IAsyncEnumerable<ElfhSupportingLookup> ExtractAsync(
        CancellationToken cancellationToken = default);
}