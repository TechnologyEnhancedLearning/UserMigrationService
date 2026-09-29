namespace LearningHub.UserMigrationService.Models.Transformation;

public class StaffGroupJobRoleTypeMapping
{
    public int Id { get; set; }

    public int LegacyStaffGroupId { get; set; }

    public string LegacyStaffGroup { get; set; } = string.Empty;

    public int? JobRoleTypeId { get; set; }

    public string JobRoleType { get; set; } = string.Empty;

    public bool IsMapped { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}