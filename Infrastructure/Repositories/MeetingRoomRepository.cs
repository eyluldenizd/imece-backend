using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class MeetingRoomRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public MeetingRoomRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<MeetingRooms>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<MeetingRooms>(
            MeetingRoomQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    public Task<MeetingRooms?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@MeetingRoomId", SqlDbType.Int) { Value = id }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<MeetingRooms>(
            MeetingRoomQueries.GetById,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByCodeInCompanyAsync(
        int companyId,
        string code,
        int excludeMeetingRoomId = 0,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@CompanyId", companyId),
            new("@Code", code),
            new("@ExcludeMeetingRoomId", excludeMeetingRoomId)
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            MeetingRoomQueries.ExistsByCode,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<int> CreateAsync(MeetingRooms entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(
            MeetingRoomQueries.Create,
            BuildParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(MeetingRooms entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            MeetingRoomQueries.Update,
            BuildParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@MeetingRoomId", id)
        ];

        return _dataAccess.ExecuteAsync(MeetingRoomQueries.SoftDelete, parameters, cancellationToken);
    }

    private static SqlParameter[] BuildParameters(MeetingRooms entity, bool includeId)
    {
        var list = new List<SqlParameter>();

        if (includeId)
        {
            list.Add(new("@MeetingRoomId", entity.MeetingRoomId));
        }

        list.Add(new("@CompanyId", entity.CompanyId));
        list.Add(new("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        list.Add(new("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));
        list.Add(new("@Name", entity.Name));
        list.Add(new("@Code", entity.Code));
        list.Add(new("@Floor", (object?)entity.Floor ?? DBNull.Value));
        list.Add(new("@Capacity", entity.Capacity));
        list.Add(new("@LocationDescription", (object?)entity.LocationDescription ?? DBNull.Value));
        list.Add(new("@Features", (object?)entity.Features ?? DBNull.Value));
        list.Add(new("@IsActive", entity.IsActive));

        return list.ToArray();
    }
}
