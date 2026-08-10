using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces
{
    public interface IMigrationStep
    {
        string StepName { get; }

        Task ExecuteAsync(
            MigrationContext context,
            CancellationToken cancellationToken);
    }
}
