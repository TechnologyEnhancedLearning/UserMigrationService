namespace LearningHub.UserMigrationService.Models;

public class MigrationStepRun
{
    public Guid MigrationStepRunId { get; set; }

    public Guid MigrationRunId { get; set; }

    public string StepName { get; set; } = string.Empty;

    public DateTime StartedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public long? DurationMs { get; set; }

    public long RecordsRead { get; set; }

    public long RecordsWritten { get; set; }

    public long RecordsSkipped { get; set; }

    public long RecordsFailed { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedUtc { get; set; }
}