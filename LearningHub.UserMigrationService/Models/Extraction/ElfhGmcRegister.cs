namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhGmcRegister
{
    public string? GmcReferenceNumber { get; set; }

    public string? Surname { get; set; }

    public string? GivenName { get; set; }

    public int? YearOfQualification { get; set; }

    public DateTime? GpRegisterDate { get; set; }

    public string? RegistrationStatus { get; set; }

    public string? OtherNames { get; set; }

    public DateTime? DateProcessed { get; set; }

    public string? Action { get; set; }
}