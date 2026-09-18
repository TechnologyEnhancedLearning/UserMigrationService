using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class OrganisationRegionResolverTests
{
    [Theory]
    [InlineData("BT1 1AA", "Northern Ireland")]
    [InlineData("AB10 1AA", "Scotland")]
    [InlineData("EH1 1AA", "Scotland")]
    [InlineData("CF10 1AA", "Wales")]
    [InlineData("LL1 1AA", "Wales")]
    [InlineData("LS1 1AA", "England")]
    public void Resolve_ReturnsExpectedRegion(
        string postcode,
        string expected)
    {
        var result =
            OrganisationRegionResolver.Resolve(postcode);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_ReturnsNullForEmptyPostcode(
        string? postcode)
    {
        var result =
            OrganisationRegionResolver.Resolve(postcode);

        Assert.Null(result);
    }
}