using LearningHub.UserMigrationService.Interfaces;
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

        public UserMigrationSelectionService(
            ILegacyRepository legacyRepository,
            ILearningHubRepository learningHubRepository)
        {
            _legacyRepository = legacyRepository;
            _learningHubRepository = learningHubRepository;
        }

        public async Task<int> PopulateUsersToMigrateAsync(
            CancellationToken cancellationToken)
        {
            // Stage 1:
            // Active eLFH users with authentication activity
            // during the last 24 months.

            var elfhUsers =
                await _legacyRepository.GetElfhUsersToMigrateAsync(
                    24,
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
