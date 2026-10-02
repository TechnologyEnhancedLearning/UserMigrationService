namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhOrganisation
{
    public int LocationId { get; set; }

    public string? LocationCode { get; set; }

    public string? LocationName { get; set; }

    public string? PostCode { get; set; }

    public int? LocationTypeId { get; set; }

    public int? ParentId { get; set; }

    public DateTimeOffset? Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public int? AmendUserId { get; set; }

    public bool Deleted { get; set; }
}