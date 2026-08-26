using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Services;
using Moq;

namespace LearningHub.UserMigrationService.Tests.Services;

public class OrganisationMigrationSelectionServiceTests
{
    private readonly Mock<ILegacyRepository> _legacyRepository;
    private readonly Mock<ILearningHubRepository> _learningHubRepository;

    private readonly OrganisationMigrationSelectionService _service;

    public OrganisationMigrationSelectionServiceTests()
    {
        _legacyRepository = new Mock<ILegacyRepository>();
        _learningHubRepository = new Mock<ILearningHubRepository>();

        _service = new OrganisationMigrationSelectionService(
            _legacyRepository.Object,
            _learningHubRepository.Object);
    }

    [Fact]
    public async Task PopulateOrganisationLocationsToMigrateAsync_ShouldMergeAdminAndEmploymentLocations()
    {
        // Arrange

        var userIds = new[]
        {
            1001,
            1002
        };

        var adminLocations = new List<int>
        {
            10,
            20,
            30
        };

        var employmentLocations = new List<int>
        {
            30,
            40,
            50
        };

        _legacyRepository
            .Setup(x => x.GetElfhAdminLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminLocations);

        _legacyRepository
            .Setup(x => x.GetElfhEmploymentLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employmentLocations);

        _learningHubRepository
            .Setup(x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act

        var result =
            await _service.PopulateOrganisationLocationsToMigrateAsync(
                userIds,
                CancellationToken.None);

        // Assert

        Assert.Equal(5, result);

        _learningHubRepository.Verify(
            x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.Is<IEnumerable<int>>(ids =>
                    ids.OrderBy(x => x).SequenceEqual(
                        new[]
                        {
                            10,
                            20,
                            30,
                            40,
                            50
                        })),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopulateOrganisationLocationsToMigrateAsync_ShouldRemoveDuplicateLocations()
    {
        // Arrange

        var userIds = new[]
        {
            1001
        };

        _legacyRepository
            .Setup(x => x.GetElfhAdminLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>
            {
                10,
                20,
                20
            });

        _legacyRepository
            .Setup(x => x.GetElfhEmploymentLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>
            {
                20,
                30,
                30
            });

        _learningHubRepository
            .Setup(x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act

        var result =
            await _service.PopulateOrganisationLocationsToMigrateAsync(
                userIds,
                CancellationToken.None);

        // Assert

        Assert.Equal(3, result);

        _learningHubRepository.Verify(
            x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.Is<IEnumerable<int>>(ids =>
                    ids.Distinct().Count() == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopulateOrganisationLocationsToMigrateAsync_ShouldReturnZero_WhenNoUsersProvided()
    {
        // Arrange

        var userIds = Enumerable.Empty<int>();

        _learningHubRepository
            .Setup(x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act

        var result =
            await _service.PopulateOrganisationLocationsToMigrateAsync(
                userIds,
                CancellationToken.None);

        // Assert

        Assert.Equal(0, result);

        _legacyRepository.Verify(
            x => x.GetElfhAdminLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _legacyRepository.Verify(
            x => x.GetElfhEmploymentLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}