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
}
