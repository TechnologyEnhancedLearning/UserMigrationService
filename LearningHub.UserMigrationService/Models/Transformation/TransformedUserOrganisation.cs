namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserOrganisation
{
    public Guid MigrationRunId { get; set; }

    public int ElfhUserId { get; set; }
    public int ElfhLocationId { get; set; }

    public int UserId { get; set; }
    public int OrganisationId { get; set; }

    public int JobRoleTypeId { get; set; }
    public string? JobRole { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset CreateDate { get; set; }
    public int? CreateUserId { get; set; }

    public DateTimeOffset? AmendDate { get; set; }
    public int? AmendUserId { get; set; }

    public DateTimeOffset? RemoveDate { get; set; }
    public int? RemoveUserId { get; set; }
    public DateTimeOffset? CreatedUtc { get; set; }
}