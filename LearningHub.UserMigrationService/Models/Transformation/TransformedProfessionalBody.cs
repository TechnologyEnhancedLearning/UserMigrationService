namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedProfessionalBody
{
    public Guid MigrationRunId { get; set; }

    // Legacy/source values
    public int LegacyProfessionalBodyId { get; set; }
    public string? LegacyProfessionalBody { get; set; }
    public string? LegacyProfessionalBodyCode { get; set; }
    public string? UploadPrefix { get; set; }

    public bool IncludeOnCerts { get; set; }
    public bool IsRemoved { get; set; }

    public int? LegacyAmendUserId { get; set; }
    public DateTimeOffset? LegacyAmendDate { get; set; }

    // Learning Hub mapping
    public int? ProfessionalBodyId { get; set; }
    public string? ProfessionalBody { get; set; }

    public bool IsMapped { get; set; }
}