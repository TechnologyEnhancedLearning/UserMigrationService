namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhUserEmployment
{
    public int UserEmploymentId { get; set; }

    public int UserId { get; set; }

    public int? LocationId { get; set; }

    public int? JobRoleId { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset? AmendDate { get; set; }

    public int? AmendUserId { get; set; }

    public bool Deleted { get; set; }
}