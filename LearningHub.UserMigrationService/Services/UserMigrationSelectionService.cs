using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Services
{
    public class UserMigrationSelectionService: IUserMigrationSelectionService
    {
        private readonly ILegacyRepository _legacyRepository;
        private readonly ILearningHubRepository _learningHubRepository;
        private readonly MigrationOptions _migrationOptions;

        public UserMigrationSelectionService(
            ILegacyRepository legacyRepository,
            ILearningHubRepository learningHubRepository,
            IOptions<MigrationOptions> migrationOptions)
        {
            _legacyRepository = legacyRepository;
            _learningHubRepository = learningHubRepository;
            _migrationOptions = migrationOptions.Value;

        }

        public async Task<int> PopulateUsersToMigrateAsync(
            CancellationToken cancellationToken)
        {
            // Stage 1:
            // Active eLFH users with authentication activity
            // during the last 24 months.

            var elfhUsers =
                await _legacyRepository.GetElfhUsersToMigrateAsync(
                    _migrationOptions.UserMigrationMonths,
                    cancellationToken);

            // Stage 2:
            // Users already having related Learning Hub data.

            var learningHubUsers =
                await _learningHubRepository.GetUsersWithExistingDataAsync(
                    cancellationToken);

            // Merge both result sets.

            var usersToMigrate =
                elfhUsers
                    .Union(learningHubUsers)
                    .Distinct()
                    .ToList();

            // Populate migration.UserIdsToMigrate

            await _learningHubRepository.InsertUsersToMigrateAsync(
                usersToMigrate,
                cancellationToken);

            return usersToMigrate.Count;
        }
    }
}
