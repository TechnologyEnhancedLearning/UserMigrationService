namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedOrganisation
{
    public Guid MigrationRunId { get; set; }

    // Legacy identifiers
    public int LegacyOrganisationId { get; set; }

    public int? LegacyOrganisationTypeId { get; set; }

    public int? LegacyParentOrganisationId { get; set; }

    // Legacy/source values
    public string? LegacyOrganisationCode { get; set; }

    public string? LegacyOrganisationName { get; set; }

    public string? LegacyPostCode { get; set; }

    // Learning Hub values
    public int? OrganisationTypeId { get; set; }

    public string? OrganisationType { get; set; }

    public string? Region { get; set; }

    // Audit
    public DateTimeOffset? CreatedUtc { get; set; }

    public DateTimeOffset? UpdatedUtc { get; set; }

    // Status
    public bool IsRemoved { get; set; }
}