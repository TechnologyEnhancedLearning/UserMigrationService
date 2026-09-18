namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserAdminLocation
{
    public Guid MigrationRunId { get; set; }

    public int LegacyUserId { get; set; }
    public int? LegacyAdminLocationId { get; set; }

    public bool IsRemoved { get; set; }
}