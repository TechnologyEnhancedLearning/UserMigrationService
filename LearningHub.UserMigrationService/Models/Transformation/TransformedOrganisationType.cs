namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedOrganisationType
{
    public Guid MigrationRunId { get; set; }

    // Legacy identifier
    public int LegacyOrganisationTypeId { get; set; }

    // Legacy value
    public string? LegacyOrganisationType { get; set; }

    // Learning Hub value
    public int? OrganisationTypeId { get; set; }

    public string? OrganisationType { get; set; }

    public bool IsMapped { get; set; }

    public bool IsRemoved { get; set; }
}