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

    public int RecordsRead { get; set; }

    public int RecordsWritten { get; set; }

    public int RecordsSkipped { get; set; }

    public int RecordsFailed { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedUtc { get; set; }
}