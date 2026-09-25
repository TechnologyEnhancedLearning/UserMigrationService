namespace LearningHub.UserMigrationService.Models;

public class MigrationStatistics
{
    public long RecordsRead { get; set; }

    public long RecordsWritten { get; set; }

    public long RecordsSkipped { get; set; }

    public long RecordsFailed { get; set; }

    public long RecordsRemoved { get; set; }
    public int RecordsUnmapped { get; set; }
}