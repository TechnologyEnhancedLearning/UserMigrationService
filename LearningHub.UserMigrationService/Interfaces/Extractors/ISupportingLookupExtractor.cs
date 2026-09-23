using LearningHub.UserMigrationService.Models.Extraction;

namespace LearningHub.UserMigrationService.Interfaces.Extractors;

public interface ISupportingLookupExtractor
{
    IAsyncEnumerable<ElfhGdcRegister> ExtractGdcAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ElfhGmcRegister> ExtractGmcAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ElfhSupportingLookup>
        ExtractOrganisationTypesAsync(
            CancellationToken cancellationToken = default);
}