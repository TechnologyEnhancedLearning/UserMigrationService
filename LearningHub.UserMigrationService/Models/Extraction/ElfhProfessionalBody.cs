namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhProfessionalBody
{
    public int ProfessionalBodyId { get; set; }

    public string? ProfessionalBody { get; set; }

    public string? ProfessionalBodyCode { get; set; }

    public string? UploadPrefix { get; set; }

    public bool IncludeOnCerts { get; set; }

    public bool Deleted { get; set; }

    public int? AmendUserId { get; set; }

    public DateTimeOffset? AmendDate { get; set; }
}