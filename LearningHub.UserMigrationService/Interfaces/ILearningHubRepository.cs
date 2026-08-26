using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Interfaces
{
    public interface ILearningHubRepository
    {
        Task<bool> TestConnectionAsync();
        Task<int> SaveUserIdsToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken);
        Task<List<int>> GetUsersWithExistingDataAsync(CancellationToken cancellationToken);

        Task InsertUsersToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken);
        Task<List<int>> GetUserIdsToMigrateAsync(CancellationToken cancellationToken);
        Task<int> InsertOrganisationLocationIdsToMigrateAsync(IEnumerable<int> locationIds,CancellationToken cancellationToken);
    }
}
