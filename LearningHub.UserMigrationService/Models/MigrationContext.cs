using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Models
{
    public class MigrationContext
    {
        public Guid MigrationRunId { get; set; }

        public string MigrationName { get; set; } = "User Migration";

        public DateTime StartedUtc { get; set; }

        public int RecordsRead { get; set; }

        public int RecordsWritten { get; set; }

        public int RecordsFailed { get; set; }
    }
}
