namespace LearningHub.UserMigrationService.Models.Transformation;

public class OrganisationTypeMapping
{
    public int Id { get; set; }

    public int LegacyOrganisationTypeId { get; set; }

    public string? LegacyOrganisationType { get; set; }

    public int? OrganisationTypeId { get; set; }

    public string? OrganisationType { get; set; }

    public bool IsMapped { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}