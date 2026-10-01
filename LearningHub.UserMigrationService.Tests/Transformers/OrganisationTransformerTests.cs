using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class OrganisationTransformerTests
{
    [Fact]
    public void Transform_PreservesLegacyIdentifiers()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            LocationTypeId = 5,
            ParentId = 20,
            LocationCode = "ORG001",
            LocationName = "Test Organisation",
            PostCode = "OX1 1AA"
        };

        var mapping = new OrganisationTypeMapping
        {
            LegacyOrganisationTypeId = 5,
            LegacyOrganisationType = "Legacy Hospital",
            OrganisationTypeId = 25,
            OrganisationType = "NHS Trust",
            IsMapped = true
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            mapping,
            Guid.NewGuid());

        Assert.Equal(
            100,
            result.ElfhLocationId);

        Assert.Equal(
            100,
            result.OrganisationId);

        Assert.Equal(
            25,
            result.OrganisationTypeId);

        Assert.Equal(
            20,
            result.ParentId);

        Assert.Equal(
            "ORG001",
            result.ODSCode);

        Assert.Equal(
            "Test Organisation",
            result.OrganisationName);

        Assert.Equal(
            "OX1 1AA",
            result.PostCode);
    }

    [Fact]
    public void Transform_AppliesOrganisationTypeMapping()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            LocationTypeId = 5,
            LocationName = "Test Organisation"
        };

        var mapping = new OrganisationTypeMapping
        {
            LegacyOrganisationTypeId = 5,
            LegacyOrganisationType = "Legacy Hospital",
            OrganisationTypeId = 25,
            OrganisationType = "NHS Trust",
            IsMapped = true
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            mapping,
            Guid.NewGuid());

        Assert.Equal(
            25,
            result.OrganisationTypeId);
    }

    [Fact]
    public void Transform_UnmappedOrganisationType_UsesZero()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            LocationTypeId = 5,
            LocationName = "Test Organisation"
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Equal(
            0,
            result.OrganisationTypeId);
    }

    [Fact]
    public void Transform_UsesCreateDate()
    {
        var created =
            new DateTime(
                2025,
                1,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc);

        var source = new ElfhOrganisation
        {
            LocationId = 100,
            Created = created
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Equal(
            new DateTimeOffset(created),
            result.CreateDate);
    }

    [Fact]
    public void Transform_UsesAmendDate()
    {
        var updated =
            new DateTime(
                2025,
                2,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc);

        var source = new ElfhOrganisation
        {
            LocationId = 100,
            Updated = updated,
            AmendUserId = 123
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Equal(
            new DateTimeOffset(updated),
            result.AmendDate);

        Assert.Equal(
            123,
            result.AmendUserId);
    }

    [Fact]
    public void Transform_DeletedOrganisation_SetsRemoveDate()
    {
        var updated =
            new DateTime(
                2025,
                2,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc);

        var source = new ElfhOrganisation
        {
            LocationId = 100,
            Deleted = true,
            Updated = updated,
            AmendUserId = 123
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.NotNull(
            result.RemoveDate);

        Assert.Equal(
            new DateTimeOffset(updated),
            result.RemoveDate);

        Assert.Equal(
            123,
            result.RemoveUserId);
    }

    [Fact]
    public void Transform_ActiveOrganisation_DoesNotSetRemoveDate()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            Deleted = false
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Null(
            result.RemoveDate);

        Assert.Null(
            result.RemoveUserId);
    }

    [Fact]
    public void Transform_NormalisesOrganisationName()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            LocationName = "  Test Organisation  "
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Equal(
            "Test Organisation",
            result.OrganisationName);
    }

    [Fact]
    public void Transform_NormalisesPostCode()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            PostCode = "  OX1 1AA  "
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        Assert.Equal(
            "OX1 1AA",
            result.PostCode);
    }

    [Fact]
    public void Transform_SetsMigrationRunId()
    {
        var migrationRunId = Guid.NewGuid();

        var source = new ElfhOrganisation
        {
            LocationId = 100
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            migrationRunId);

        Assert.Equal(
            migrationRunId,
            result.MigrationRunId);
    }

    [Fact]
    public void Transform_SetsCreatedUtc()
    {
        var before =
            DateTime.UtcNow;

        var source = new ElfhOrganisation
        {
            LocationId = 100
        };

        var transformer = new OrganisationTransformer();

        var result = transformer.Transform(
            source,
            null,
            Guid.NewGuid());

        var after =
            DateTime.UtcNow;

        Assert.InRange(
            result.CreatedUtc,
            before,
            after);
    }
}