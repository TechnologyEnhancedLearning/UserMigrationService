namespace LearningHub.UserMigrationService.Models.Transformation;

public class ProfessionalBodyMapping
{
    public int Id { get; set; }

    public int LegacyProfessionalBodyId { get; set; }

    public string? LegacyProfessionalBody { get; set; }

    public int? ProfessionalBodyId { get; set; }

    public string? ProfessionalBody { get; set; }

    public bool IsMapped { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }
}