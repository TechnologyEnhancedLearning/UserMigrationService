using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Services;
using Moq;
using LearningHub.UserMigrationService.Configuration;
using Microsoft.Extensions.Options;

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

        var migrationOptions =
                     Options.Create(
                        new MigrationOptions
                         {
                             BatchSize = 1000
                         });

        _service = new OrganisationMigrationSelectionService(
            _legacyRepository.Object,
            _learningHubRepository.Object,
            migrationOptions);
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
    [Fact]
    public async Task PopulateOrganisationLocationsToMigrateAsync_ShouldProcessUsersInBatches()
    {
        // Arrange

        var userIds = Enumerable.Range(1, 2501).ToArray();

        var adminBatchSizes = new List<int>();
        var employmentBatchSizes = new List<int>();

        _legacyRepository
            .Setup(x => x.GetElfhAdminLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<int>, CancellationToken>(
                (ids, _) =>
                {
                    adminBatchSizes.Add(ids.Count());
                })
            .ReturnsAsync(new List<int>());

        _legacyRepository
            .Setup(x => x.GetElfhEmploymentLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<int>, CancellationToken>(
                (ids, _) =>
                {
                    employmentBatchSizes.Add(ids.Count());
                })
            .ReturnsAsync(new List<int>());

        _learningHubRepository
            .Setup(x => x.InsertOrganisationLocationIdsToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act

        await _service.PopulateOrganisationLocationsToMigrateAsync(
            userIds,
            CancellationToken.None);

        // Assert

        Assert.Equal(
            new[] { 1000, 1000, 501 },
            adminBatchSizes);

        Assert.Equal(
            new[] { 1000, 1000, 501 },
            employmentBatchSizes);
    }
}