using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserEmploymentTransformerTests
{
    [Fact]
    public void Transform_MapsEmployment()
    {
        var source = new ElfhUserEmployment
        {
            UserEmploymentId = 10,
            UserId = 100,
            LocationId = 200,
            JobRoleId = 300,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2025, 1, 1),
            AmendDate = new DateTime(2025, 2, 1),
            AmendUserId = 500,
            Deleted = false
        };

        var transformer =
            new UserEmploymentTransformer();

        var result =
            transformer.Transform(
                source,
                Guid.NewGuid());

        Assert.Equal(
            10,
            result.LegacyUserEmploymentId);

        Assert.Equal(
            100,
            result.LegacyUserId);

        Assert.Equal(
            200,
            result.LegacyLocationId);

        Assert.Equal(
            300,
            result.LegacyJobRoleId);

        Assert.False(result.IsRemoved);
        Assert.Equal(500, result.LegacyAmendUserId);
    }
}