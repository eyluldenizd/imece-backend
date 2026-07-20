using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ServiceRouteRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ServiceRouteRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<ServiceRoutes>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<ServiceRoutes>(ServiceRouteQueries.GetAll, null, cancellationToken);

    public Task<ServiceRoutes?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceRouteId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<ServiceRoutes>(
            ServiceRouteQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(ServiceRoutes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<long>(
            ServiceRouteQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(ServiceRoutes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            ServiceRouteQueries.Update,
            UpdateWriteParameters(entity),
            cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ServiceRouteId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.ExecuteAsync(ServiceRouteQueries.SoftDelete, parameters, cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(ServiceRoutes entity)
    {
        return
        [
            new("@RouteName", entity.RouteName),
            new("@DepartureLocation", entity.DepartureLocation),
            new("@ArrivalLocation", entity.ArrivalLocation),
            new("@DepartureLocationId", (object?)entity.DepartureLocationId ?? DBNull.Value),
            new("@ArrivalLocationId", (object?)entity.ArrivalLocationId ?? DBNull.Value),
            new("@RouteDescription", (object?)entity.RouteDescription ?? DBNull.Value),
            new("@DepartureTime", (object?)entity.DepartureTime ?? DBNull.Value),
            new("@ArrivalTime", (object?)entity.ArrivalTime ?? DBNull.Value),
            new("@IsActive", entity.IsActive),
            new("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }

    private static SqlParameter[] UpdateWriteParameters(ServiceRoutes entity)
    {
        return
        [
            new("@ServiceRouteId", SqlDbType.BigInt) { Value = entity.ServiceRouteId },
            new("@RouteName", entity.RouteName),
            new("@DepartureLocation", entity.DepartureLocation),
            new("@ArrivalLocation", entity.ArrivalLocation),
            new("@DepartureLocationId", (object?)entity.DepartureLocationId ?? DBNull.Value),
            new("@ArrivalLocationId", (object?)entity.ArrivalLocationId ?? DBNull.Value),
            new("@RouteDescription", (object?)entity.RouteDescription ?? DBNull.Value),
            new("@DepartureTime", (object?)entity.DepartureTime ?? DBNull.Value),
            new("@ArrivalTime", (object?)entity.ArrivalTime ?? DBNull.Value),
            new("@IsActive", entity.IsActive),
            new("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }
}
