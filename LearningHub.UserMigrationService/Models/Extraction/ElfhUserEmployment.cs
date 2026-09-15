namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhUserEmployment
{
    public int UserEmploymentId { get; set; }

    public int UserId { get; set; }

    public int? LocationId { get; set; }

    public int? JobRoleId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? AmendDate { get; set; }

    public int? AmendUserId { get; set; }

    public bool Deleted { get; set; }
}