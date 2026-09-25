using LearningHub.UserMigrationService.Interfaces;

namespace LearningHub.UserMigrationService.Services;

using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using Microsoft.Extensions.Options;
public class OrganisationMigrationSelectionService
    : IOrganisationMigrationSelectionService
{
    private readonly ILegacyRepository _legacyRepository;
    private readonly ILearningHubRepository _learningHubRepository;
    private readonly int _batchSize;

    public OrganisationMigrationSelectionService(
        ILegacyRepository legacyRepository,
        ILearningHubRepository learningHubRepository,
        IOptions<MigrationOptions> migrationOptions)
    {
        _legacyRepository = legacyRepository;
        _learningHubRepository = learningHubRepository;
        _batchSize = migrationOptions.Value.BatchSize;
        if (_batchSize <= 0)
        {
            throw new InvalidOperationException(
                "MigrationOptions:BatchSize must be greater than zero.");
        }
    }

    public async Task<int>
    PopulateOrganisationLocationsToMigrateAsync(
        IEnumerable<int> userIds,
        CancellationToken cancellationToken)
    {
        var locationIds =
            new HashSet<int>();

        foreach (var userBatch in userIds.Distinct().Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var adminLocationIds =
                await _legacyRepository
                    .GetElfhAdminLocationIdsToMigrateAsync(
                        userBatch,
                        cancellationToken);

            foreach (var locationId in adminLocationIds)
            {
                locationIds.Add(locationId);
            }

            var employmentLocationIds =
                await _legacyRepository
                    .GetElfhEmploymentLocationIdsToMigrateAsync(
                        userBatch,
                        cancellationToken);

            foreach (var locationId in employmentLocationIds)
            {
                locationIds.Add(locationId);
            }
        }

        return await _learningHubRepository
            .InsertOrganisationLocationIdsToMigrateAsync(
                locationIds,
                cancellationToken);
    }
}