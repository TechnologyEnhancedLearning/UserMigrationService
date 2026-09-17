using LearningHub.UserMigrationService.Models.Extraction;
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

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                "Hospital",
                Guid.NewGuid());

        Assert.Equal(
            100,
            result.LegacyOrganisationId);

        Assert.Equal(
            5,
            result.LegacyOrganisationTypeId);

        Assert.Equal(
            20,
            result.LegacyParentOrganisationId);
    }

    [Fact]
    public void Transform_InfersEnglandFromPostcode()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            PostCode = "OX1 1AA"
        };

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.Equal(
            "England",
            result.Region);
    }

    [Fact]
    public void Transform_InfersScotlandFromPostcode()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            PostCode = "EH1 1AA"
        };

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.Equal(
            "Scotland",
            result.Region);
    }

    [Fact]
    public void Transform_InfersWalesFromPostcode()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            PostCode = "CF10 1AA"
        };

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.Equal(
            "Wales",
            result.Region);
    }

    [Fact]
    public void Transform_InfersNorthernIrelandFromPostcode()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            PostCode = "BT1 1AA"
        };

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.Equal(
            "Northern Ireland",
            result.Region);
    }

    [Fact]
    public void Transform_DeletedOrganisation_IsRemoved()
    {
        var source = new ElfhOrganisation
        {
            LocationId = 100,
            Deleted = true
        };

        var transformer =
            new OrganisationTransformer();

        var result =
            transformer.Transform(
                source,
                null,
                Guid.NewGuid());

        Assert.True(result.IsRemoved);
    }
}