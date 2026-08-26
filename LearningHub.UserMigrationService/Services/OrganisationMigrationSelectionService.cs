using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Services;

public class OrganisationMigrationSelectionService
    : IOrganisationMigrationSelectionService
{
    private readonly ILegacyRepository _legacyRepository;
    private readonly ILearningHubRepository _learningHubRepository;

    public OrganisationMigrationSelectionService(
        ILegacyRepository legacyRepository,
        ILearningHubRepository learningHubRepository)
    {
        _legacyRepository = legacyRepository;
        _learningHubRepository = learningHubRepository;
    }

    public async Task<int> PopulateOrganisationLocationsToMigrateAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds?
            .Distinct()
            .ToList()
            ?? new List<int>();

        // Nothing to process.
        if (ids.Count == 0)
        {
            await _learningHubRepository
                .InsertOrganisationLocationIdsToMigrateAsync(
                    Enumerable.Empty<int>(),
                    cancellationToken);

            return 0;
        }

        // Stage 1:
        // Select locations from eLFH User Admin Locations.

        var adminLocationIds =
            await _legacyRepository
                .GetElfhAdminLocationIdsToMigrateAsync(
                    ids,
                    cancellationToken)
            ?? new List<int>();

        // Stage 2:
        // Select locations from eLFH User Employments.

        var employmentLocationIds =
            await _legacyRepository
                .GetElfhEmploymentLocationIdsToMigrateAsync(
                    ids,
                    cancellationToken)
            ?? new List<int>();

        // Merge both result sets.

        var organisationLocationIds =
            adminLocationIds
                .Union(employmentLocationIds)
                .Distinct()
                .ToList();

        // Populate migrations.OrganisationLocationIdsToMigrate.

        await _learningHubRepository
            .InsertOrganisationLocationIdsToMigrateAsync(
                organisationLocationIds,
                cancellationToken);

        return organisationLocationIds.Count;
    }
}