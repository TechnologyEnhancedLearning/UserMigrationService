namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserGroupRole
{
    public Guid MigrationRunId { get; set; }

    public int LegacyUserId { get; set; }

    public int LegacyUserGroupId { get; set; }

    public int? LegacyUserGroupReporterId { get; set; }

    public int RoleId { get; set; }

    public bool IsRemoved { get; set; }
}