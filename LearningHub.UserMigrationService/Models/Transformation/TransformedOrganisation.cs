namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedOrganisation
{
    public Guid MigrationRunId { get; set; }

    public int ElfhLocationId { get; set; }

    public int OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    public string? ODSCode { get; set; }

    public string? PostCode { get; set; }

    public int OrganisationTypeId { get; set; }

    public int? RegionId { get; set; }

    public int? ParentId { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public int? CreateUserId { get; set; }

    public DateTimeOffset? AmendDate { get; set; }

    public int? AmendUserId { get; set; }

    public DateTimeOffset? RemoveDate { get; set; }

    public int? RemoveUserId { get; set; }

    public DateTime CreatedUtc { get; set; }
}