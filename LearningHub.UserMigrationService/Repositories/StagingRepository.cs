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
    public async Task InsertGdcRegistersAsync(
    IEnumerable<TransformedGdcRegister> records,
    CancellationToken cancellationToken)
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add("MigrationRunId", typeof(Guid));
        dataTable.Columns.Add("RegistrationNumber", typeof(string));
        dataTable.Columns.Add("Dentist", typeof(bool));
        dataTable.Columns.Add("Title", typeof(string));
        dataTable.Columns.Add("Surname", typeof(string));
        dataTable.Columns.Add("Forenames", typeof(string));
        dataTable.Columns.Add("Honorifics", typeof(string));
        dataTable.Columns.Add("HouseName", typeof(string));
        dataTable.Columns.Add("AddressLine1", typeof(string));
        dataTable.Columns.Add("AddressLine2", typeof(string));
        dataTable.Columns.Add("AddressLine3", typeof(string));
        dataTable.Columns.Add("AddressLine4", typeof(string));
        dataTable.Columns.Add("Town", typeof(string));
        dataTable.Columns.Add("County", typeof(string));
        dataTable.Columns.Add("PostCode", typeof(string));
        dataTable.Columns.Add("Country", typeof(string));
        dataTable.Columns.Add("RegistrationDate", typeof(DateTimeOffset));
        dataTable.Columns.Add("Qualifications", typeof(string));
        dataTable.Columns.Add("DcpTitles", typeof(string));
        dataTable.Columns.Add("Specialties", typeof(string));
        dataTable.Columns.Add("Condition", typeof(string));
        dataTable.Columns.Add("Suspension", typeof(string));
        dataTable.Columns.Add("DateProcessed", typeof(DateTimeOffset));
        dataTable.Columns.Add("Action", typeof(string));

        foreach (var record in records)
        {
            var row = dataTable.NewRow();

            row["MigrationRunId"] =
                record.MigrationRunId;

            row["RegistrationNumber"] =
                (object?)record.RegistrationNumber
                ?? DBNull.Value;

            row["Dentist"] =
                record.Dentist;

            row["Title"] =
                (object?)record.Title
                ?? DBNull.Value;

            row["Surname"] =
                (object?)record.Surname
                ?? DBNull.Value;

            row["Forenames"] =
                (object?)record.Forenames
                ?? DBNull.Value;

            row["Honorifics"] =
                (object?)record.Honorifics
                ?? DBNull.Value;

            row["HouseName"] =
                (object?)record.HouseName
                ?? DBNull.Value;

            row["AddressLine1"] =
                (object?)record.AddressLine1
                ?? DBNull.Value;

            row["AddressLine2"] =
                (object?)record.AddressLine2
                ?? DBNull.Value;

            row["AddressLine3"] =
                (object?)record.AddressLine3
                ?? DBNull.Value;

            row["AddressLine4"] =
                (object?)record.AddressLine4
                ?? DBNull.Value;

            row["Town"] =
                (object?)record.Town
                ?? DBNull.Value;

            row["County"] =
                (object?)record.County
                ?? DBNull.Value;

            row["PostCode"] =
                (object?)record.PostCode
                ?? DBNull.Value;

            row["Country"] =
                (object?)record.Country
                ?? DBNull.Value;

            row["RegistrationDate"] =
                (object?)record.RegistrationDate
                ?? DBNull.Value;

            row["Qualifications"] =
                (object?)record.Qualifications
                ?? DBNull.Value;

            row["DcpTitles"] =
                (object?)record.DcpTitles
                ?? DBNull.Value;

            row["Specialties"] =
                (object?)record.Specialties
                ?? DBNull.Value;

            row["Condition"] =
                (object?)record.Condition
                ?? DBNull.Value;

            row["Suspension"] =
                (object?)record.Suspension
                ?? DBNull.Value;

            row["DateProcessed"] =
                (object?)record.DateProcessed
                ?? DBNull.Value;

            row["Action"] =
                (object?)record.Action
                ?? DBNull.Value;

            dataTable.Rows.Add(row);
        }

        if (dataTable.Rows.Count == 0)
            return;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        using var bulkCopy =
            new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock,
                null);

        bulkCopy.DestinationTableName =
            "[migrations].[GdcRegister]";

        bulkCopy.BatchSize =
            _bulkCopyBatchSize;

        bulkCopy.BulkCopyTimeout =
            _bulkCopyTimeoutSeconds;

        bulkCopy.EnableStreaming = true;

        // IMPORTANT:
        // Explicitly map every column by name.
        // Do not rely on ordinal/position matching.
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(
                column.ColumnName,
                column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(
            dataTable,
            cancellationToken);
    }
    public async Task InsertGmcRegistersAsync(
    IEnumerable<TransformedGmcRegister> records,
    CancellationToken cancellationToken)
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add(
            "MigrationRunId",
            typeof(Guid));

        dataTable.Columns.Add(
            "GMC_Ref_No",
            typeof(string));

        dataTable.Columns.Add(
            "Surname",
            typeof(string));

        dataTable.Columns.Add(
            "Given_Name",
            typeof(string));

        dataTable.Columns.Add(
            "Year_Of_Qualification",
            typeof(double));

        dataTable.Columns.Add(
            "GP_Register_Date",
            typeof(string));

        dataTable.Columns.Add(
            "Registration_Status",
            typeof(string));

        dataTable.Columns.Add(
            "Other_Names",
            typeof(string));

        dataTable.Columns.Add(
            "DateProcessed",
            typeof(DateTimeOffset));

        dataTable.Columns.Add(
            "Action",
            typeof(string));

        foreach (var record in records)
        {
            var row = dataTable.NewRow();

            row["MigrationRunId"] =
                record.MigrationRunId;

            row["GMC_Ref_No"] =
                (object?)record.GmcReferenceNumber
                ?? DBNull.Value;

            row["Surname"] =
                (object?)record.Surname
                ?? DBNull.Value;

            row["Given_Name"] =
                (object?)record.GivenName
                ?? DBNull.Value;

            row["Year_Of_Qualification"] =
                (object?)record.YearOfQualification
                ?? DBNull.Value;

            row["GP_Register_Date"] =
                (object?)record.GpRegisterDate
                ?? DBNull.Value;

            row["Registration_Status"] =
                (object?)record.RegistrationStatus
                ?? DBNull.Value;

            row["Other_Names"] =
                (object?)record.OtherNames
                ?? DBNull.Value;

            row["DateProcessed"] =
                record.DateProcessed;

            row["Action"] =
                (object?)record.Action
                ?? DBNull.Value;

            dataTable.Rows.Add(row);
        }

        if (dataTable.Rows.Count == 0)
            return;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        using var bulkCopy =
            new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock,
                null);

        bulkCopy.DestinationTableName =
            "[migrations].[GmcLrmp]";

        bulkCopy.BatchSize =
            _bulkCopyBatchSize;

        bulkCopy.BulkCopyTimeout =
            _bulkCopyTimeoutSeconds;

        bulkCopy.EnableStreaming = true;

        // IMPORTANT:
        // Explicitly map source DataTable columns
        // to destination SQL columns.
        //
        // This also means the identity column [Id]
        // and default column [CreatedUtc] are ignored.
        bulkCopy.ColumnMappings.Add(
            "MigrationRunId",
            "MigrationRunId");

        bulkCopy.ColumnMappings.Add(
            "GMC_Ref_No",
            "GMC_Ref_No");

        bulkCopy.ColumnMappings.Add(
            "Surname",
            "Surname");

        bulkCopy.ColumnMappings.Add(
            "Given_Name",
            "Given_Name");

        bulkCopy.ColumnMappings.Add(
            "Year_Of_Qualification",
            "Year_Of_Qualification");

        bulkCopy.ColumnMappings.Add(
            "GP_Register_Date",
            "GP_Register_Date");

        bulkCopy.ColumnMappings.Add(
            "Registration_Status",
            "Registration_Status");

        bulkCopy.ColumnMappings.Add(
            "Other_Names",
            "Other_Names");

        bulkCopy.ColumnMappings.Add(
            "DateProcessed",
            "DateProcessed");

        bulkCopy.ColumnMappings.Add(
            "Action",
            "Action");

        await bulkCopy.WriteToServerAsync(
            dataTable,
            cancellationToken);
    }
}