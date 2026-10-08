namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedGmcRegister
{
    public Guid MigrationRunId { get; set; }

    public string GmcReferenceNumber { get; set; } = string.Empty;

    public string? Surname { get; set; }

    public string? GivenName { get; set; }

    public double? YearOfQualification { get; set; }

    public string? GpRegisterDate { get; set; }

    public string? RegistrationStatus { get; set; }

    public string? OtherNames { get; set; }

    public DateTimeOffset DateProcessed { get; set; }

    public string? Action { get; set; }
}