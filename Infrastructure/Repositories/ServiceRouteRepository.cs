using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ServiceRouteRepository
{
    private readonly DbManager _dbManager;

    public ServiceRouteRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<ServiceRoutes>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dbManager.QueryAsync<ServiceRoutes>(ServiceRouteQueries.GetAll, null, cancellationToken);

    public Task<ServiceRoutes?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@ServiceRouteId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.QueryFirstOrDefaultAsync<ServiceRoutes>(
            ServiceRouteQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(ServiceRoutes entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(ServiceRouteQueries.Create, CreateWriteParameters(entity), cancellationToken);

    public Task<int> UpdateAsync(ServiceRoutes entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(ServiceRouteQueries.Update, UpdateWriteParameters(entity), cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@ServiceRouteId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.ExecuteAsync(ServiceRouteQueries.Delete, parameters, cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(ServiceRoutes entity)
    {
        return
        [
            new SqlParameter("@RouteName", entity.RouteName),
            new SqlParameter("@DepartureLocation", entity.DepartureLocation),
            new SqlParameter("@ArrivalLocation", entity.ArrivalLocation),
            new SqlParameter("@RouteDescription", (object?)entity.RouteDescription ?? DBNull.Value),
            new SqlParameter("@DepartureTime", (object?)entity.DepartureTime ?? DBNull.Value),
            new SqlParameter("@ArrivalTime", (object?)entity.ArrivalTime ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }

    private static SqlParameter[] UpdateWriteParameters(ServiceRoutes entity)
    {
        return
        [
            new SqlParameter("@ServiceRouteId", SqlDbType.BigInt) { Value = entity.ServiceRouteId },
            new SqlParameter("@RouteName", entity.RouteName),
            new SqlParameter("@DepartureLocation", entity.DepartureLocation),
            new SqlParameter("@ArrivalLocation", entity.ArrivalLocation),
            new SqlParameter("@RouteDescription", (object?)entity.RouteDescription ?? DBNull.Value),
            new SqlParameter("@DepartureTime", (object?)entity.DepartureTime ?? DBNull.Value),
            new SqlParameter("@ArrivalTime", (object?)entity.ArrivalTime ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }
}
