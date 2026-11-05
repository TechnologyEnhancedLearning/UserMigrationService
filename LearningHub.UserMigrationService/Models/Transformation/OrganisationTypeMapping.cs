
namespace LearningHub.UserMigrationService.Models.Transformation;

public class OrganisationTypeMapping
{
    public int Id { get; set; }

    public int LegacyOrganisationTypeId { get; set; }

    public string LegacyOrganisationType { get; set; } = string.Empty;

    public int OrganisationTypeId { get; set; }

    public string OrganisationType { get; set; } = string.Empty;

    public int MappingConfidence { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}
