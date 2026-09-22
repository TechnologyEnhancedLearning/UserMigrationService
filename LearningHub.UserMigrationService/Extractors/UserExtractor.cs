using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class UserExtractor : IUserExtractor
{
    private readonly string _connectionString;

    public UserExtractor(IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhUser> ExtractAsync(
        IEnumerable<int> userIds,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var ids = userIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            yield break;
        }

        var (parameters, parameterNames) =
            SqlParameterHelper.CreateIntParameters(
                ids,
                "UserId");

        var sql = $"""
            SELECT
                u.userId,
                u.firstName,
                u.lastName,
                u.emailAddress,
                u.altEmailAddress,
                u.userName,
                u.registrationCode,
                u.active,
                u.passwordHash,
                u.mustChangeNextLogin,
                u.passwordLifeCounter,
                u.RemoteLoginKey,
                u.RemoteLoginGuid,
                u.RemoteLoginStart,
                u.RestrictToSSO,
                u.createdDate,
                u.amendDate,
                u.amendUserId,
                u.deleted
            FROM dbo.userTBL AS u
            WHERE u.userId IN ({parameterNames})
            ORDER BY u.userId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        command.Parameters.AddRange(
            parameters.ToArray());

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var userIdOrdinal =
            reader.GetOrdinal("userId");

        var firstNameOrdinal =
            reader.GetOrdinal("firstName");

        var lastNameOrdinal =
            reader.GetOrdinal("lastName");

        var emailAddressOrdinal =
            reader.GetOrdinal("emailAddress");

        var altEmailAddressOrdinal =
            reader.GetOrdinal("altEmailAddress");

        var userNameOrdinal =
            reader.GetOrdinal("userName");

        var registrationCodeOrdinal =
            reader.GetOrdinal("registrationCode");

        var activeOrdinal =
            reader.GetOrdinal("active");

        var passwordHashOrdinal =
            reader.GetOrdinal("passwordHash");

        var mustChangeNextLoginOrdinal =
            reader.GetOrdinal("mustChangeNextLogin");

        var passwordLifeCounterOrdinal =
            reader.GetOrdinal("passwordLifeCounter");

        var remoteLoginKeyOrdinal =
            reader.GetOrdinal("RemoteLoginKey");

        var remoteLoginGuidOrdinal =
            reader.GetOrdinal("RemoteLoginGuid");

        var remoteLoginStartOrdinal =
            reader.GetOrdinal("RemoteLoginStart");

        var restrictToSsoOrdinal =
            reader.GetOrdinal("RestrictToSSO");

        var createdDateOrdinal =
            reader.GetOrdinal("createdDate");

        var amendDateOrdinal =
            reader.GetOrdinal("amendDate");

        var amendUserIdOrdinal =
            reader.GetOrdinal("amendUserId");

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhUser
            {
                UserId =
                    reader.GetInt32(userIdOrdinal),

                FirstName =
                    GetNullableString(
                        reader,
                        firstNameOrdinal),

                LastName =
                    GetNullableString(
                        reader,
                        lastNameOrdinal),

                EmailAddress =
                    GetNullableString(
                        reader,
                        emailAddressOrdinal),

                AltEmailAddress =
                    GetNullableString(
                        reader,
                        altEmailAddressOrdinal),

                UserName =
                    GetNullableString(
                        reader,
                        userNameOrdinal),

                RegistrationCode =
                    GetNullableString(
                        reader,
                        registrationCodeOrdinal),

                Active =
                    GetBoolean(
                        reader,
                        activeOrdinal),

                PasswordHash =
                    GetNullableString(
                        reader,
                        passwordHashOrdinal),

                MustChangeNextLogin =
                    GetBoolean(
                        reader,
                        mustChangeNextLoginOrdinal),

                PasswordLifeCounter =
                    reader.IsDBNull(passwordLifeCounterOrdinal)
                        ? null
                        : reader.GetInt32(passwordLifeCounterOrdinal),

                RemoteLoginKey =
                    GetNullableString(
                        reader,
                        remoteLoginKeyOrdinal),

                RemoteLoginGuid =
                    reader.IsDBNull(remoteLoginGuidOrdinal)
                        ? null
                        : reader.GetGuid(remoteLoginGuidOrdinal),

                RemoteLoginStart =
                    reader.IsDBNull(remoteLoginStartOrdinal)
                        ? null
                        : reader.GetDateTime(remoteLoginStartOrdinal),

                RestrictToSSO =
                    GetBoolean(
                        reader,
                        restrictToSsoOrdinal),

                CreatedDate =
                    reader.IsDBNull(createdDateOrdinal)
                        ? null
                        : reader.GetDateTime(createdDateOrdinal),

                AmendDate =
                    reader.IsDBNull(amendDateOrdinal)
                        ? null
                        : reader.GetDateTime(amendDateOrdinal),

                AmendUserId =
                    reader.IsDBNull(amendUserIdOrdinal)
                        ? null
                        : reader.GetInt32(amendUserIdOrdinal),

                Deleted =
                    GetBoolean(
                        reader,
                        deletedOrdinal)
            };
        }
    }

    private static string? GetNullableString(
        SqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static bool GetBoolean(
        SqlDataReader reader,
        int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        var value = reader.GetValue(ordinal);

        return value switch
        {
            bool b => b,
            byte b => b != 0,
            short s => s != 0,
            int i => i != 0,
            long l => l != 0,
            _ => Convert.ToBoolean(value)
        };
    }
}