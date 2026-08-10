using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningHub.UserMigrationService.Models;

public class MigrationRun
{
    public Guid MigrationRunId { get; set; }

    public string MigrationName { get; set; } = string.Empty;

    public DateTime StartedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? StartedBy { get; set; }

    public string? MigrationAppVersion { get; set; }

    public long? TotalDurationMs { get; set; }

    public int TotalSteps { get; set; }

    public int SuccessfulSteps { get; set; }

    public int FailedSteps { get; set; }

    public int TotalRecordsRead { get; set; }

    public int TotalRecordsWritten { get; set; }

    public int TotalRecordsSkipped { get; set; }

    public int TotalRecordsFailed { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedUtc { get; set; }
}