namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUser
{
    public Guid MigrationRunId { get; set; }

    // Legacy identity
    public int LegacyUserId { get; set; }

    // User details
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? AltEmailAddress { get; set; }
    public string? UserName { get; set; }
    public string? RegistrationCode { get; set; }

    // Authentication/account state
    public bool IsActive { get; set; }
    public bool IsRemoved { get; set; }

    public string? PasswordHash { get; set; }
    public bool MustChangeNextLogin { get; set; }
    public int? PasswordLifeCounter { get; set; }

    public string? RemoteLoginKey { get; set; }
    public Guid? RemoteLoginGuid { get; set; }
    public DateTimeOffset? RemoteLoginStart { get; set; }

    public bool RestrictToSso { get; set; }

    // Audit
    public DateTimeOffset? CreatedUtc { get; set; }
    public DateTimeOffset? UpdatedUtc { get; set; }
    public int? LegacyAmendUserId { get; set; }
}