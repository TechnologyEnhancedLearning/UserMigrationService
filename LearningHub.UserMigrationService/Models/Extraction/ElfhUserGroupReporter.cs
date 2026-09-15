namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhUserGroupReporter
{
    public int UserGroupReporterId { get; set; }

    public int UserId { get; set; }

    public int UserGroupId { get; set; }

    public bool Deleted { get; set; }

    public int? AmendUserId { get; set; }

    public DateTime? AmendDate { get; set; }
}