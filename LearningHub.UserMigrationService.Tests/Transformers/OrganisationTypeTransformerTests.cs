using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class OrganisationTypeTransformerTests
{
    [Fact]
    public void Transform_AppliesMappedOrganisationType()
    {
        var source =
            new ElfhSupportingLookup
            {
                LookupType = "OrganisationType",
                Id = 10,
                Name = "Legacy Hospital"
            };

        var transformer =
            new OrganisationTypeTransformer();

        var result =
            transformer.Transform(
                source,
                25,
                "NHS Trust",
                true,
                Guid.NewGuid());

        Assert.Equal(
            10,
            result.LegacyOrganisationTypeId);

        Assert.Equal(
            "Legacy Hospital",
            result.LegacyOrganisationType);

        Assert.Equal(
            25,
            result.OrganisationTypeId);

        Assert.Equal(
            "NHS Trust",
            result.OrganisationType);

        Assert.True(
            result.IsMapped);
    }

    [Fact]
    public void Transform_UnmappedOrganisationType_IsNotMapped()
    {
        var source =
            new ElfhSupportingLookup
            {
                LookupType = "OrganisationType",
                Id = 10,
                Name = "Unknown Legacy Type"
            };

        var transformer =
            new OrganisationTypeTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                null,
                false,
                Guid.NewGuid());

        Assert.Equal(
            10,
            result.LegacyOrganisationTypeId);

        Assert.Null(
            result.OrganisationTypeId);

        Assert.Null(
            result.OrganisationType);

        Assert.False(
            result.IsMapped);
    }
}