namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserEmployment
{
    public Guid MigrationRunId { get; set; }

    // Legacy identity
    public int LegacyUserEmploymentId { get; set; }
    public int LegacyUserId { get; set; }

    // Relationships
    public int? LegacyLocationId { get; set; }
    public int? LegacyJobRoleId { get; set; }

    // Employment
    public DateTimeOffset? StartDateUtc { get; set; }
    public DateTimeOffset? EndDateUtc { get; set; }

    // Audit
    public DateTimeOffset? UpdatedUtc { get; set; }
    public int? LegacyAmendUserId { get; set; }

    // Status
    public bool IsRemoved { get; set; }
}