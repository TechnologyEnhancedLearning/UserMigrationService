using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Configuration
{
    public class DatabaseOptions
    {
        public string LegacyHubConnectionString { get; set; } = string.Empty;

        public string LearningHubConnectionString { get; set; } = string.Empty;
    }
}
