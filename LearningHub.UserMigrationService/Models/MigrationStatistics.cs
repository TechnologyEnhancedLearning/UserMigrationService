namespace LearningHub.UserMigrationService.Models;

public class MigrationStatistics
{
    public int RecordsRead { get; set; }

    public int RecordsWritten { get; set; }

    public int RecordsSkipped { get; set; }

    public int RecordsFailed { get; set; }
}