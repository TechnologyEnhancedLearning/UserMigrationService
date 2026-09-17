using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class ProfessionalBodyTransformerTests
{
    [Fact]
    public void Transform_WithMapping_CreatesMappedProfessionalBody()
    {
        var migrationRunId = Guid.NewGuid();

        var source = new ElfhProfessionalBody
        {
            ProfessionalBodyId = 10,
            ProfessionalBody = "General Dental Council",
            ProfessionalBodyCode = "GDC",
            UploadPrefix = "GDC",
            IncludeOnCerts = true,
            Deleted = false,
            AmendUserId = 123,
            AmendDate = new DateTime(
                2026,
                1,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc)
        };

        var mapping = new ProfessionalBodyMapping
        {
            LegacyProfessionalBodyId = 10,
            LegacyProfessionalBody = "General Dental Council",
            ProfessionalBodyId = 25,
            ProfessionalBody = "General Dental Council",
            IsMapped = true
        };

        var transformer =
            new ProfessionalBodyTransformer();

        var result =
            transformer.Transform(
                source,
                mapping,
                migrationRunId);

        Assert.Equal(
            migrationRunId,
            result.MigrationRunId);

        Assert.Equal(
            10,
            result.LegacyProfessionalBodyId);

        Assert.Equal(
            25,
            result.ProfessionalBodyId);

        Assert.Equal(
            "General Dental Council",
            result.ProfessionalBody);

        Assert.True(result.IsMapped);
        Assert.False(result.IsRemoved);
        Assert.True(result.IncludeOnCerts);
    }

    [Fact]
    public void Transform_WithoutMapping_IsUnmapped()
    {
        var migrationRunId = Guid.NewGuid();

        var source = new ElfhProfessionalBody
        {
            ProfessionalBodyId = 10,
            ProfessionalBody = "Unknown Body",
            Deleted = false
        };

        var transformer =
            new ProfessionalBodyTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                migrationRunId);

        Assert.Equal(
            10,
            result.LegacyProfessionalBodyId);

        Assert.Null(
            result.ProfessionalBodyId);

        Assert.Null(
            result.ProfessionalBody);

        Assert.False(
            result.IsMapped);
    }

    [Fact]
    public void Transform_DeletedSource_SetsIsRemoved()
    {
        var source = new ElfhProfessionalBody
        {
            ProfessionalBodyId = 10,
            ProfessionalBody = "Old Body",
            Deleted = true
        };

        var transformer =
            new ProfessionalBodyTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.True(
            result.IsRemoved);
    }
    [Fact]
    public void Transform_ConvertsAmendDateToUtc()
    {
        var source = new ElfhProfessionalBody
        {
            ProfessionalBodyId = 10,
            AmendDate = new DateTime(
                2026,
                1,
                10,
                12,
                0,
                0)
        };

        var transformer =
            new ProfessionalBodyTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.NotNull(result.LegacyAmendDate);

        Assert.Equal(
            DateTimeKind.Utc,
            result.LegacyAmendDate!.Value.UtcDateTime.Kind);

        Assert.Equal(
            new DateTime(
                2026,
                1,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc),
            result.LegacyAmendDate.Value.UtcDateTime);
    }
    [Fact]
    public void Transform_NullAmendDate_RemainsNull()
    {
        var source = new ElfhProfessionalBody
        {
            ProfessionalBodyId = 10,
            AmendDate = null
        };

        var transformer =
            new ProfessionalBodyTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.Null(result.LegacyAmendDate);
    }
}