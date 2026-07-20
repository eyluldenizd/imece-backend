using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CompanyRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CompanyRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Companies>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<Companies>(CompanyQueries.GetAll, null, cancellationToken);

    public Task<List<Companies>> GetActiveAsync(
        CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<Companies>(CompanyQueries.GetActive, null, cancellationToken);

    public Task<Companies?> GetByIdAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Companies>(
            CompanyQueries.GetById,
            parameters,
            cancellationToken);
    }

    public async Task<string?> GetCompanyNameByIdAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
        ];

        var result = await _dataAccess.ExecuteScalarAsync<object?>(
            CompanyQueries.GetCompanyNameById,
            parameters,
            cancellationToken);

        return result as string;
    }

    public async Task<bool> ExistsByCodeAsync(
        string companyCode,
        int? excludeCompanyId = null,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyCode", SqlDbType.NVarChar, 64) { Value = companyCode },
            new SqlParameter("@ExcludeCompanyId", SqlDbType.Int)
            {
                Value = excludeCompanyId.HasValue ? excludeCompanyId.Value : DBNull.Value
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            CompanyQueries.ExistsByCode,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<int> CreateAsync(
        Companies entity,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<int>(
            CompanyQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(
        Companies entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity).ToList();
        parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = entity.CompanyId });
        return _dataAccess.ExecuteAsync(CompanyQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
        ];

        return _dataAccess.ExecuteAsync(
            CompanyQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(Companies entity) =>
    [
        new SqlParameter("@CompanyCode", SqlDbType.NVarChar, 64) { Value = entity.CompanyCode },
        new SqlParameter("@CompanyName", SqlDbType.NVarChar, 256) { Value = entity.CompanyName },
        new SqlParameter("@Description", SqlDbType.NVarChar, 1024)
        {
            Value = entity.Description ?? (object)DBNull.Value
        },
        new SqlParameter("@LogoUrl", SqlDbType.NVarChar, 1024)
        {
            Value = entity.LogoUrl ?? (object)DBNull.Value
        },
        new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
    ];
}
