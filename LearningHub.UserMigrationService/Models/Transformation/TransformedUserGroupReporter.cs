namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserGroupReporter
{
    public Guid MigrationRunId { get; set; }

    public int LegacyUserGroupReporterId { get; set; }

    public int LegacyUserId { get; set; }

    public int LegacyUserGroupId { get; set; }

    public bool IsRemoved { get; set; }

    public int? LegacyAmendUserId { get; set; }

    public DateTimeOffset? UpdatedUtc { get; set; }
}