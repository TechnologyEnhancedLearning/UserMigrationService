using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserGroupReporterTransformerTests
{
    [Fact]
    public void Transform_MapsGroupReporter()
    {
        var source = new ElfhUserGroupReporter
        {
            UserGroupReporterId = 10,
            UserId = 100,
            UserGroupId = 200,
            Deleted = false,
            AmendUserId = 500,
            AmendDate = new DateTime(2025, 1, 1)
        };

        var transformer =
            new UserGroupReporterTransformer();

        var result =
            transformer.Transform(
                source,
                Guid.NewGuid());

        Assert.Equal(
            10,
            result.LegacyUserGroupReporterId);

        Assert.Equal(
            100,
            result.LegacyUserId);

        Assert.Equal(
            200,
            result.LegacyUserGroupId);

        Assert.False(result.IsRemoved);

        Assert.Equal(
            500,
            result.LegacyAmendUserId);
    }
}