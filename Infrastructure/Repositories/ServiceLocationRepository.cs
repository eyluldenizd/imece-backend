using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ServiceLocationRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ServiceLocationRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<ServiceLocations>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<ServiceLocations>(
            ServiceLocationQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    public Task<ServiceLocations?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceLocationId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<ServiceLocations>(
            ServiceLocationQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(ServiceLocations entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<long>(
            ServiceLocationQueries.Create,
            BuildParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(ServiceLocations entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            ServiceLocationQueries.Update,
            BuildParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceLocationId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.ExecuteAsync(ServiceLocationQueries.SoftDelete, parameters, cancellationToken);
    }

    private static SqlParameter[] BuildParameters(ServiceLocations entity, bool includeId)
    {
        var list = new List<SqlParameter>();

        if (includeId)
        {
            list.Add(new("@ServiceLocationId", entity.ServiceLocationId));
        }

        list.Add(new("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));
        list.Add(new("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        list.Add(new("@Name", entity.Name));
        list.Add(new("@ServiceLocationTypeId", (object?)entity.ServiceLocationTypeId ?? DBNull.Value));
        list.Add(new("@LocationType", entity.LocationType));
        list.Add(new("@Address", (object?)entity.Address ?? DBNull.Value));
        list.Add(new("@Latitude", (object?)entity.Latitude ?? DBNull.Value));
        list.Add(new("@Longitude", (object?)entity.Longitude ?? DBNull.Value));
        list.Add(new("@IsActive", entity.IsActive));

        return list.ToArray();
    }
}

public sealed class ServiceRouteStopRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ServiceRouteStopRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<ServiceRouteStops>> GetByRouteIdAsync(
        long serviceRouteId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceRouteId", SqlDbType.BigInt) { Value = serviceRouteId }
        ];

        return _dataAccess.QueryAsync<ServiceRouteStops>(
            ServiceRouteStopQueries.GetByRouteId,
            parameters,
            cancellationToken: cancellationToken);
    }

    public Task DeleteByRouteIdAsync(long serviceRouteId, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceRouteId", SqlDbType.BigInt) { Value = serviceRouteId }
        ];

        return _dataAccess.ExecuteAsync(
            ServiceRouteStopQueries.DeleteByRouteId,
            parameters,
            cancellationToken);
    }

    public Task CreateAsync(ServiceRouteStops entity, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceRouteId", entity.ServiceRouteId),
            new("@ServiceLocationId", entity.ServiceLocationId),
            new("@StopOrder", entity.StopOrder),
            new("@ArrivalTime", (object?)entity.ArrivalTime ?? DBNull.Value),
            new("@DepartureTime", (object?)entity.DepartureTime ?? DBNull.Value),
            new("@Notes", (object?)entity.Notes ?? DBNull.Value),
            new("@IsActive", entity.IsActive)
        ];

        return _dataAccess.ExecuteAsync(ServiceRouteStopQueries.Create, parameters, cancellationToken);
    }
}
