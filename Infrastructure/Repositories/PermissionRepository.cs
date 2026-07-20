using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class PermissionRecord
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class PermissionRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public PermissionRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<PermissionRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<PermissionRecord>(PermissionQueries.GetAll, null, cancellationToken);

    public async Task<int> CountExistingIdsAsync(
        IReadOnlyList<int> permissionIds,
        CancellationToken cancellationToken = default)
    {
        if (permissionIds.Count == 0)
        {
            return 0;
        }

        var placeholders = string.Join(", ", permissionIds.Select((_, index) => $"@P{index}"));
        var sql = string.Format(PermissionQueries.GetByIdsTemplate, placeholders);

        var parameters = permissionIds
            .Select((id, index) => new Microsoft.Data.SqlClient.SqlParameter($"@P{index}", id))
            .ToArray();

        var rows = await _dataAccess.QueryAsync<PermissionIdRow>(sql, parameters, cancellationToken);
        return rows.Count;
    }

    private sealed class PermissionIdRow
    {
        public int PermissionId { get; set; }
    }
}
