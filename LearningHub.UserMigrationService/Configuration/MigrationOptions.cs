using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Configuration;

public class MigrationOptions
{
    public int UserMigrationMonths { get; set; } = 24;
    public int BatchSize { get; set; } = 5000;
    public bool EnableValidation { get; set; } = true;
}
