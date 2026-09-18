using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserAdminLocationTransformerTests
{
    [Fact]
    public void Transform_MapsAdminLocation()
    {
        var source = new ElfhUserAdminLocation
        {
            UserId = 100,
            AdminLocationId = 500,
            Deleted = false
        };

        var transformer =
            new UserAdminLocationTransformer();

        var result =
            transformer.Transform(
                source,
                Guid.NewGuid());

        Assert.Equal(
            100,
            result.LegacyUserId);

        Assert.Equal(
            500,
            result.LegacyAdminLocationId);

        Assert.False(result.IsRemoved);
    }
}