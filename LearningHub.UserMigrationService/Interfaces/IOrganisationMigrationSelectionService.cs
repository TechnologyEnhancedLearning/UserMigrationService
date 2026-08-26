using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IOrganisationMigrationSelectionService
{
    Task<int> PopulateOrganisationLocationsToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken);
}