using Microsoft.Data.SqlClient;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

internal static class SqlParameterHelper
{
    public static (
        List<SqlParameter> Parameters,
        string ParameterNames) CreateIntParameters(
        IEnumerable<int> values,
        string parameterPrefix)
    {
        var distinctValues = values
            .Distinct()
            .ToList();

        var parameters = new List<SqlParameter>();

        for (var i = 0; i < distinctValues.Count; i++)
        {
            parameters.Add(
                new SqlParameter(
                    $"@{parameterPrefix}{i}",
                    SqlDbType.Int)
                {
                    Value = distinctValues[i]
                });
        }

        var parameterNames = string.Join(
            ", ",
            parameters.Select(x => x.ParameterName));

        return (parameters, parameterNames);
    }
}