using System.Data;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class StagingRepository : IStagingRepository
{
    private readonly string _connectionString;

    public StagingRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

    public async Task InsertUsersAsync(
        IReadOnlyCollection<TransformedUser> users,
        CancellationToken cancellationToken = default)
    {
        if (users.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyUserId", typeof(int));
        table.Columns.Add("FirstName", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("EmailAddress", typeof(string));
        table.Columns.Add("AltEmailAddress", typeof(string));
        table.Columns.Add("UserName", typeof(string));
        table.Columns.Add("RegistrationCode", typeof(string));
        table.Columns.Add("IsActive", typeof(bool));
        table.Columns.Add("IsRemoved", typeof(bool));
        table.Columns.Add("PasswordHash", typeof(string));
        table.Columns.Add("MustChangeNextLogin", typeof(bool));
        table.Columns.Add("PasswordLifeCounter", typeof(int));
        table.Columns.Add("RemoteLoginKey", typeof(string));
        table.Columns.Add("RemoteLoginGuid", typeof(Guid));
        table.Columns.Add("RemoteLoginStart", typeof(DateTimeOffset));
        table.Columns.Add("RestrictToSso", typeof(bool));
        table.Columns.Add("CreatedUtc", typeof(DateTimeOffset));
        table.Columns.Add("UpdatedUtc", typeof(DateTimeOffset));
        table.Columns.Add("LegacyAmendUserId", typeof(int));

        foreach (var user in users)
        {
            table.Rows.Add(
                user.MigrationRunId,
                user.LegacyUserId,
                DbValue(user.FirstName),
                DbValue(user.LastName),
                DbValue(user.EmailAddress),
                DbValue(user.AltEmailAddress),
                DbValue(user.UserName),
                DbValue(user.RegistrationCode),
                user.IsActive,
                user.IsRemoved,
                DbValue(user.PasswordHash),
                user.MustChangeNextLogin,
                DbValue(user.PasswordLifeCounter),
                DbValue(user.RemoteLoginKey),
                DbValue(user.RemoteLoginGuid),
                DbValue(user.RemoteLoginStart),
                user.RestrictToSso,
                DbValue(user.CreatedUtc),
                DbValue(user.UpdatedUtc),
                DbValue(user.LegacyAmendUserId));
        }

        await BulkInsertAsync(
            table,
            "[migrations].[User]",
            cancellationToken);
    }

    public async Task InsertUserEmploymentsAsync(
        IReadOnlyCollection<TransformedUserEmployment> employments,
        CancellationToken cancellationToken = default)
    {
        if (employments.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyUserEmploymentId", typeof(int));
        table.Columns.Add("LegacyUserId", typeof(int));
        table.Columns.Add("LegacyLocationId", typeof(int));
        table.Columns.Add("LegacyJobRoleId", typeof(int));
        table.Columns.Add("StartDateUtc", typeof(DateTimeOffset));
        table.Columns.Add("EndDateUtc", typeof(DateTimeOffset));
        table.Columns.Add("UpdatedUtc", typeof(DateTimeOffset));
        table.Columns.Add("LegacyAmendUserId", typeof(int));
        table.Columns.Add("IsRemoved", typeof(bool));

        foreach (var employment in employments)
        {
            table.Rows.Add(
                employment.MigrationRunId,
                employment.LegacyUserEmploymentId,
                employment.LegacyUserId,
                DbValue(employment.LegacyLocationId),
                DbValue(employment.LegacyJobRoleId),
                DbValue(employment.StartDateUtc),
                DbValue(employment.EndDateUtc),
                DbValue(employment.UpdatedUtc),
                DbValue(employment.LegacyAmendUserId),
                employment.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserEmployment]",
            cancellationToken);
    }

    public async Task InsertUserAdminLocationsAsync(
        IReadOnlyCollection<TransformedUserAdminLocation> adminLocations,
        CancellationToken cancellationToken = default)
    {
        if (adminLocations.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyUserId", typeof(int));
        table.Columns.Add("LegacyAdminLocationId", typeof(int));
        table.Columns.Add("IsRemoved", typeof(bool));

        foreach (var record in adminLocations)
        {
            table.Rows.Add(
                record.MigrationRunId,
                record.LegacyUserId,
                DbValue(record.LegacyAdminLocationId),
                record.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserAdminLocation]",
            cancellationToken);
    }

    public async Task InsertUserGroupReportersAsync(
        IReadOnlyCollection<TransformedUserGroupReporter> groupReporters,
        CancellationToken cancellationToken = default)
    {
        if (groupReporters.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyUserGroupReporterId", typeof(int));
        table.Columns.Add("LegacyUserId", typeof(int));
        table.Columns.Add("LegacyUserGroupId", typeof(int));
        table.Columns.Add("IsRemoved", typeof(bool));
        table.Columns.Add("LegacyAmendUserId", typeof(int));
        table.Columns.Add("UpdatedUtc", typeof(DateTimeOffset));

        foreach (var record in groupReporters)
        {
            table.Rows.Add(
                record.MigrationRunId,
                record.LegacyUserGroupReporterId,
                record.LegacyUserId,
                record.LegacyUserGroupId,
                record.IsRemoved,
                DbValue(record.LegacyAmendUserId),
                DbValue(record.UpdatedUtc));
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserGroupReporter]",
            cancellationToken);
    }

    public async Task InsertOrganisationsAsync(
        IReadOnlyCollection<TransformedOrganisation> organisations,
        CancellationToken cancellationToken = default)
    {
        if (organisations.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyOrganisationId", typeof(int));
        table.Columns.Add("LegacyOrganisationTypeId", typeof(int));
        table.Columns.Add("LegacyParentOrganisationId", typeof(int));
        table.Columns.Add("LegacyOrganisationCode", typeof(string));
        table.Columns.Add("LegacyOrganisationName", typeof(string));
        table.Columns.Add("LegacyPostCode", typeof(string));
        table.Columns.Add("OrganisationTypeId", typeof(int));
        table.Columns.Add("OrganisationType", typeof(string));
        table.Columns.Add("Region", typeof(string));
        table.Columns.Add("CreatedUtc", typeof(DateTimeOffset));
        table.Columns.Add("UpdatedUtc", typeof(DateTimeOffset));
        table.Columns.Add("IsRemoved", typeof(bool));

        foreach (var organisation in organisations)
        {
            table.Rows.Add(
                organisation.MigrationRunId,
                organisation.LegacyOrganisationId,
                DbValue(organisation.LegacyOrganisationTypeId),
                DbValue(organisation.LegacyParentOrganisationId),
                DbValue(organisation.LegacyOrganisationCode),
                DbValue(organisation.LegacyOrganisationName),
                DbValue(organisation.LegacyPostCode),
                DbValue(organisation.OrganisationTypeId),
                DbValue(organisation.OrganisationType),
                DbValue(organisation.Region),
                DbValue(organisation.CreatedUtc),
                DbValue(organisation.UpdatedUtc),
                organisation.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[Organisation]",
            cancellationToken);
    }

    public async Task InsertOrganisationTypesAsync(
        IReadOnlyCollection<TransformedOrganisationType> organisationTypes,
        CancellationToken cancellationToken = default)
    {
        if (organisationTypes.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyOrganisationTypeId", typeof(int));
        table.Columns.Add("LegacyOrganisationType", typeof(string));
        table.Columns.Add("OrganisationTypeId", typeof(int));
        table.Columns.Add("OrganisationType", typeof(string));
        table.Columns.Add("IsMapped", typeof(bool));
        table.Columns.Add("IsRemoved", typeof(bool));

        foreach (var organisationType in organisationTypes)
        {
            table.Rows.Add(
                organisationType.MigrationRunId,
                organisationType.LegacyOrganisationTypeId,
                DbValue(organisationType.LegacyOrganisationType),
                DbValue(organisationType.OrganisationTypeId),
                DbValue(organisationType.OrganisationType),
                organisationType.IsMapped,
                organisationType.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[OrganisationType]",
            cancellationToken);
    }

    public async Task InsertProfessionalBodiesAsync(
        IReadOnlyCollection<TransformedProfessionalBody> professionalBodies,
        CancellationToken cancellationToken = default)
    {
        if (professionalBodies.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyProfessionalBodyId", typeof(int));
        table.Columns.Add("LegacyProfessionalBody", typeof(string));
        table.Columns.Add("LegacyProfessionalBodyCode", typeof(string));
        table.Columns.Add("UploadPrefix", typeof(string));
        table.Columns.Add("IncludeOnCerts", typeof(bool));
        table.Columns.Add("IsRemoved", typeof(bool));
        table.Columns.Add("LegacyAmendUserId", typeof(int));
        table.Columns.Add("LegacyAmendDate", typeof(DateTimeOffset));
        table.Columns.Add("ProfessionalBodyId", typeof(int));
        table.Columns.Add("ProfessionalBody", typeof(string));
        table.Columns.Add("IsMapped", typeof(bool));

        foreach (var professionalBody in professionalBodies)
        {
            table.Rows.Add(
                professionalBody.MigrationRunId,
                professionalBody.LegacyProfessionalBodyId,
                DbValue(professionalBody.LegacyProfessionalBody),
                DbValue(professionalBody.LegacyProfessionalBodyCode),
                DbValue(professionalBody.UploadPrefix),
                professionalBody.IncludeOnCerts,
                professionalBody.IsRemoved,
                DbValue(professionalBody.LegacyAmendUserId),
                DbValue(professionalBody.LegacyAmendDate),
                DbValue(professionalBody.ProfessionalBodyId),
                DbValue(professionalBody.ProfessionalBody),
                professionalBody.IsMapped);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[ProfessionalBody]",
            cancellationToken);
    }

    private async Task BulkInsertAsync(
        DataTable table,
        string destinationTable,
        CancellationToken cancellationToken)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            using var bulkCopy =
                new SqlBulkCopy(
                    connection,
                    SqlBulkCopyOptions.CheckConstraints,
                    transaction)
                {
                    DestinationTableName = destinationTable,
                    BatchSize = table.Rows.Count,
                    BulkCopyTimeout = 0
                };

            foreach (DataColumn column in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(
                    column.ColumnName,
                    column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(
                table,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static object DbValue(
        object? value)
    {
        return value ?? DBNull.Value;
    }
}