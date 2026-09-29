namespace LearningHub.UserMigrationService.Models.Transformation;

public class TransformedUser
{
    public Guid MigrationRunId { get; set; }

    public int ElfhUserId { get; set; }

    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? EmailAddress { get; set; }

    public string? RecoveryEmailAddress { get; set; }

    public string ElfhUserName { get; set; } = string.Empty;

    public int? ProfessionalBodyId { get; set; }

    public string? ProfessionalRegistrationNumber { get; set; }

    public bool? Active { get; set; }

    public string? PasswordHash { get; set; }

    public bool? MustChangePassword { get; set; }

    public int? PasswordLifeCounter { get; set; }

    public int? SecurityLifeCounter { get; set; }

    public string? RemoteLoginKey { get; set; }

    public Guid? RemoteLoginGuid { get; set; }

    public DateTimeOffset? RemoteLoginStart { get; set; }

    public bool? RestrictToSSO { get; set; }

    public bool? RequestUserLogout { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public int? CreateUserId { get; set; }

    public DateTimeOffset? AmendDate { get; set; }

    public int? AmendUserId { get; set; }

    public DateTimeOffset? RemoveDate { get; set; }

    public int? RemoveUserId { get; set; }

    public int? RemovalMethodId { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
}