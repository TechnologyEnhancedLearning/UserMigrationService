namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedGdcRegister
{
    public Guid MigrationRunId { get; set; }

    public string? RegistrationNumber { get; set; }

    public bool Dentist { get; set; }

    public string? Title { get; set; }

    public string? Surname { get; set; }

    public string? Forenames { get; set; }

    public string? Honorifics { get; set; }

    public string? HouseName { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string? AddressLine4 { get; set; }

    public string? Town { get; set; }

    public string? County { get; set; }

    public string? PostCode { get; set; }

    public string? Country { get; set; }

    public DateTimeOffset? RegistrationDate { get; set; }

    public string? Qualifications { get; set; }

    public string? DcpTitles { get; set; }

    public string? Specialties { get; set; }

    public string? Condition { get; set; }

    public string? Suspension { get; set; }

    public DateTimeOffset? DateProcessed { get; set; }

    public string? Action { get; set; }
}