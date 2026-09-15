namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhUser
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? EmailAddress { get; set; }

    public string? AltEmailAddress { get; set; }

    public string? UserName { get; set; }

    public string? RegistrationCode { get; set; }

    public bool Active { get; set; }

    public string? PasswordHash { get; set; }

    public bool MustChangeNextLogin { get; set; }

    public int? PasswordLifeCounter { get; set; }

    public string? RemoteLoginKey { get; set; }

    public Guid? RemoteLoginGuid { get; set; }

    public DateTime? RemoteLoginStart { get; set; }

    public bool RestrictToSSO { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? AmendDate { get; set; }

    public int? AmendUserId { get; set; }

    public bool Deleted { get; set; }
}