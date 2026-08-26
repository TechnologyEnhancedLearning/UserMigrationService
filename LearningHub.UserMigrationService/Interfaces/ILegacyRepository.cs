using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Interfaces
{
    public interface ILegacyRepository
    {
        Task<bool> TestConnectionAsync();
        Task<List<int>> GetElfhUsersToMigrateAsync( int months,CancellationToken cancellationToken);
        Task<List<int>> GetElfhAdminLocationIdsToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken);

        Task<List<int>> GetElfhEmploymentLocationIdsToMigrateAsync(IEnumerable<int> userIds, CancellationToken cancellationToken);
    }
}
