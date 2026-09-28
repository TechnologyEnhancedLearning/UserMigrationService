namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUserOrganisation
{
    public Guid MigrationRunId { get; set; }

    public int LegacyUserId { get; set; }

    public int LegacyOrganisationId { get; set; }

    public int? LegacyUserEmploymentId { get; set; }

    public bool IsRemoved { get; set; }
}