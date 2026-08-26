using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Services
{
    public interface IUserMigrationSelectionService
    {
        Task<int> PopulateUsersToMigrateAsync(CancellationToken cancellationToken);
    }
}
