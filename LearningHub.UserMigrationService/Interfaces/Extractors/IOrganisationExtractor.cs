using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface IOrganisationExtractor
{
    IAsyncEnumerable<ElfhOrganisation> ExtractAsync(
        IEnumerable<int> locationIds,
        CancellationToken cancellationToken = default);
}