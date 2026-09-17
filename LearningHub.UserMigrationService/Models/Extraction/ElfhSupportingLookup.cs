namespace LearningHub.UserMigrationService.Models.Extraction;

public class ElfhSupportingLookup
{
    public string LookupType { get; set; } = string.Empty;

    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public bool Deleted { get; set; }

}