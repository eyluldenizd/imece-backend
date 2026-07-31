using System.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

internal static class CompanyListFilterParameters
{
    public static SqlParameter[] Create(CompanyListFilter filter)
    {
        return
        [
            new SqlParameter("@CompanyId", SqlDbType.Int)
            {
                Value = filter.CompanyId.HasValue ? filter.CompanyId.Value : DBNull.Value
            },
            new SqlParameter("@AccessibleCompanyIds", SqlDbType.NVarChar, -1)
            {
                Value = string.IsNullOrWhiteSpace(filter.AccessibleCompanyIdsCsv)
                    ? DBNull.Value
                    : filter.AccessibleCompanyIdsCsv
            }
        ];
    }

    public static SqlParameter[] Combine(CompanyListFilter filter, params SqlParameter[] extra)
    {
        var parameters = Create(filter);
        if (extra.Length == 0)
        {
            return parameters;
        }

        var combined = new SqlParameter[parameters.Length + extra.Length];
        parameters.CopyTo(combined, 0);
        extra.CopyTo(combined, parameters.Length);
        return combined;
    }
}
