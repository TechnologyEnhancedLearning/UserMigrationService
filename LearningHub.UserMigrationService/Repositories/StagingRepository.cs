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
    private readonly int _bulkCopyBatchSize;
    private readonly int _bulkCopyTimeoutSeconds;

    public StagingRepository(
        IOptions<DatabaseOptions> databaseOptions,
        IOptions<MigrationOptions> migrationOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;

        _bulkCopyBatchSize =
       migrationOptions.Value.BulkCopyBatchSize;

        _bulkCopyTimeoutSeconds =
            migrationOptions.Value.BulkCopyTimeoutSeconds;

        if (_bulkCopyBatchSize <= 0)
        {
            throw new InvalidOperationException(
                "MigrationOptions:BulkCopyBatchSize must be greater than zero.");
        }

        if (_bulkCopyTimeoutSeconds < 0)
        {
            throw new InvalidOperationException(
                "MigrationOptions:BulkCopyTimeoutSeconds cannot be negative.");
        }
    }

    public async Task InsertUsersAsync(
    IReadOnlyCollection<TransformedUser> users,
    CancellationToken cancellationToken = default)
    {
        if (users.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("ElfhUserId", typeof(int));
        table.Columns.Add("UserId", typeof(int));
        table.Columns.Add("FirstName", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("EmailAddress", typeof(string));
        table.Columns.Add("RecoveryEmailAddress", typeof(string));
        table.Columns.Add("ElfhUserName", typeof(string));
        table.Columns.Add("ProfessionalBodyId", typeof(int));
        table.Columns.Add("ProfessionalRegistrationNumber", typeof(string));
        table.Columns.Add("Active", typeof(bool));
        table.Columns.Add("PasswordHash", typeof(string));
        table.Columns.Add("MustChangePassword", typeof(bool));
        table.Columns.Add("PasswordLifeCounter", typeof(int));
        table.Columns.Add("SecurityLifeCounter", typeof(int));
        table.Columns.Add("RemoteLoginKey", typeof(string));
        table.Columns.Add("RemoteLoginGuid", typeof(Guid));
        table.Columns.Add("RemoteLoginStart", typeof(DateTimeOffset));
        table.Columns.Add("RestrictToSSO", typeof(bool));
        table.Columns.Add("RequestUserLogout", typeof(bool));
        table.Columns.Add("CreateDate", typeof(DateTimeOffset));
        table.Columns.Add("CreateUserId", typeof(int));
        table.Columns.Add("AmendDate", typeof(DateTimeOffset));
        table.Columns.Add("AmendUserId", typeof(int));
        table.Columns.Add("RemoveDate", typeof(DateTimeOffset));
        table.Columns.Add("RemoveUserId", typeof(int));
        table.Columns.Add("RemovalMethodId", typeof(int));
        table.Columns.Add("CreatedUtc", typeof(DateTime));

        foreach (var user in users)
        {
            table.Rows.Add(
                user.MigrationRunId,
                user.ElfhUserId,
                user.UserId,
                DbValue(user.FirstName),
                DbValue(user.LastName),
                DbValue(user.EmailAddress),
                DbValue(user.RecoveryEmailAddress),
                user.ElfhUserName,
                DbValue(user.ProfessionalBodyId),
                DbValue(user.ProfessionalRegistrationNumber),
                DbValue(user.Active),
                DbValue(user.PasswordHash),
                DbValue(user.MustChangePassword),
                DbValue(user.PasswordLifeCounter),
                DbValue(user.SecurityLifeCounter),
                DbValue(user.RemoteLoginKey),
                DbValue(user.RemoteLoginGuid),
                DbValue(user.RemoteLoginStart),
                DbValue(user.RestrictToSSO),
                DbValue(user.RequestUserLogout),
                user.CreateDate,
                DbValue(user.CreateUserId),
                DbValue(user.AmendDate),
                DbValue(user.AmendUserId),
                DbValue(user.RemoveDate),
                DbValue(user.RemoveUserId),
                DbValue(user.RemovalMethodId),
                user.CreatedUtc);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[Users]",
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
        table.Columns.Add("ElfhLocationId", typeof(int));
        table.Columns.Add("OrganisationId", typeof(int));
        table.Columns.Add("OrganisationName", typeof(string));
        table.Columns.Add("ODSCode", typeof(string));
        table.Columns.Add("PostCode", typeof(string));
        table.Columns.Add("OrganisationTypeId", typeof(int));
        table.Columns.Add("RegionId", typeof(int));
        table.Columns.Add("ParentId", typeof(int));
        table.Columns.Add("CreateDate", typeof(DateTimeOffset));
        table.Columns.Add("CreateUserId", typeof(int));
        table.Columns.Add("AmendDate", typeof(DateTimeOffset));
        table.Columns.Add("AmendUserId", typeof(int));
        table.Columns.Add("RemoveDate", typeof(DateTimeOffset));
        table.Columns.Add("RemoveUserId", typeof(int));
        table.Columns.Add("CreatedUtc", typeof(DateTime));

        foreach (var organisation in organisations)
        {
            table.Rows.Add(
                organisation.MigrationRunId,
                organisation.ElfhLocationId,
                organisation.OrganisationId,
                DbValue(organisation.OrganisationName),
                DbValue(organisation.ODSCode),
                DbValue(organisation.PostCode),
                organisation.OrganisationTypeId,
                DbValue(organisation.RegionId),
                DbValue(organisation.ParentId),
                organisation.CreateDate,
                DbValue(organisation.CreateUserId),
                DbValue(organisation.AmendDate),
                DbValue(organisation.AmendUserId),
                DbValue(organisation.RemoveDate),
                DbValue(organisation.RemoveUserId),
                organisation.CreatedUtc);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[Organisations]",
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
    public async Task InsertUserOrganisationsAsync(
    IReadOnlyCollection<TransformedUserOrganisation> userOrganisations,
    CancellationToken cancellationToken = default)
    {
        if (userOrganisations.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("ElfhUserId", typeof(int));
        table.Columns.Add("ElfhLocationId", typeof(int));
        table.Columns.Add("UserId", typeof(int));
        table.Columns.Add("OrganisationId", typeof(int));
        table.Columns.Add("JobRoleTypeId", typeof(int));
        table.Columns.Add("JobRole", typeof(string));
        table.Columns.Add("StartDate", typeof(DateTimeOffset));
        table.Columns.Add("EndDate", typeof(DateTimeOffset));
        table.Columns.Add("CreateDate", typeof(DateTimeOffset));
        table.Columns.Add("CreateUserId", typeof(int));
        table.Columns.Add("AmendDate", typeof(DateTimeOffset));
        table.Columns.Add("AmendUserId", typeof(int));
        table.Columns.Add("RemoveDate", typeof(DateTimeOffset));
        table.Columns.Add("RemoveUserId", typeof(int));
        table.Columns.Add("CreatedUtc", typeof(DateTime));

        foreach (var record in userOrganisations)
        {
            table.Rows.Add(
                record.MigrationRunId,
                record.ElfhUserId,
                record.ElfhLocationId,
                record.UserId,
                record.OrganisationId,
                record.JobRoleTypeId,
                DbValue(record.JobRole),
                DbValue(record.StartDate),
                DbValue(record.EndDate),
                record.CreateDate,
                DbValue(record.CreateUserId),
                DbValue(record.AmendDate),
                DbValue(record.AmendUserId),
                DbValue(record.RemoveDate),
                DbValue(record.RemoveUserId),
                record.CreatedUtc);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserOrganisations]",
            cancellationToken);
    }
    public async Task InsertUserRolesAsync(
    IReadOnlyCollection<TransformedUserRole> userRoles,
    CancellationToken cancellationToken = default)
    {
        if (userRoles.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add(
            "MigrationRunId",
            typeof(Guid));

        table.Columns.Add(
            "LegacyUserId",
            typeof(int));

        table.Columns.Add(
            "LegacyAdminLocationId",
            typeof(int));

        table.Columns.Add(
            "RoleId",
            typeof(int));

        table.Columns.Add(
            "IsRemoved",
            typeof(bool));

        foreach (var record in userRoles)
        {
            table.Rows.Add(
                record.MigrationRunId,
                record.LegacyUserId,
                DbValue(record.LegacyAdminLocationId),
                record.RoleId,
                record.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserRole]",
            cancellationToken);
    }
    public async Task InsertUserGroupRolesAsync(
    IReadOnlyCollection<TransformedUserGroupRole> userGroupRoles,
    CancellationToken cancellationToken = default)
    {
        if (userGroupRoles.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add(
            "MigrationRunId",
            typeof(Guid));

        table.Columns.Add(
            "LegacyUserId",
            typeof(int));

        table.Columns.Add(
            "LegacyUserGroupId",
            typeof(int));

        table.Columns.Add(
            "LegacyUserGroupReporterId",
            typeof(int));

        table.Columns.Add(
            "RoleId",
            typeof(int));

        table.Columns.Add(
            "IsRemoved",
            typeof(bool));

        foreach (var record in userGroupRoles)
        {
            table.Rows.Add(
                record.MigrationRunId,
                record.LegacyUserId,
                record.LegacyUserGroupId,
                DbValue(record.LegacyUserGroupReporterId),
                record.RoleId,
                record.IsRemoved);
        }

        await BulkInsertAsync(
            table,
            "[migrations].[UserGroupRole]",
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

                        BatchSize = _bulkCopyBatchSize,

                        BulkCopyTimeout = _bulkCopyTimeoutSeconds,

                        EnableStreaming = true
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