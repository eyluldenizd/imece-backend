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


    public Task<List<ServiceRoutes>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync(
            ServiceRouteQueries.GetAll,
            reader => new ServiceRoutes
            {
                ServiceRouteId = reader.GetInt64(
                    reader.GetOrdinal("service_route_id")),

                RouteName = reader.GetString(
                    reader.GetOrdinal("route_name")),

                DepartureLocation = reader.GetString(
                    reader.GetOrdinal("departure_location")),

                ArrivalLocation = reader.GetString(
                    reader.GetOrdinal("arrival_location")),

                RouteDescription = reader.IsDBNull(
                    reader.GetOrdinal("route_description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("route_description")),

                DepartureTime = reader.IsDBNull(
                    reader.GetOrdinal("departure_time"))
                    ? null
                    : reader.GetTimeSpan(
                        reader.GetOrdinal("departure_time")),

                ArrivalTime = reader.IsDBNull(
                    reader.GetOrdinal("arrival_time"))
                    ? null
                    : reader.GetTimeSpan(
                        reader.GetOrdinal("arrival_time")),

                IsActive = reader.GetBoolean(
                    reader.GetOrdinal("is_active")),

                DisplayOrder = reader.IsDBNull(
                    reader.GetOrdinal("display_order"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("display_order")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("created_at")),

                UpdatedAt = reader.IsDBNull(
                    reader.GetOrdinal("updated_at"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("updated_at"))
            },
            cancellationToken: cancellationToken);
    }


    public Task<ServiceRoutes?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@ServiceRouteId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QueryFirstOrDefaultAsync(
            ServiceRouteQueries.GetById,
            reader => new ServiceRoutes
            {
                ServiceRouteId = reader.GetInt64(
                    reader.GetOrdinal("service_route_id")),

                RouteName = reader.GetString(
                    reader.GetOrdinal("route_name")),

                DepartureLocation = reader.GetString(
                    reader.GetOrdinal("departure_location")),

                ArrivalLocation = reader.GetString(
                    reader.GetOrdinal("arrival_location")),

                RouteDescription = reader.IsDBNull(
                    reader.GetOrdinal("route_description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("route_description")),

                DepartureTime = reader.IsDBNull(
                    reader.GetOrdinal("departure_time"))
                    ? null
                    : reader.GetTimeSpan(
                        reader.GetOrdinal("departure_time")),

                ArrivalTime = reader.IsDBNull(
                    reader.GetOrdinal("arrival_time"))
                    ? null
                    : reader.GetTimeSpan(
                        reader.GetOrdinal("arrival_time")),

                IsActive = reader.GetBoolean(
                    reader.GetOrdinal("is_active")),

                DisplayOrder = reader.IsDBNull(
                    reader.GetOrdinal("display_order"))
                    ? null
                    : reader.GetInt32(
                        reader.GetOrdinal("display_order")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("created_at")),

                UpdatedAt = reader.IsDBNull(
                    reader.GetOrdinal("updated_at"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("updated_at"))
            },
            parameters,
            cancellationToken);
    }


    public Task<int> CreateAsync(
        ServiceRoutes entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@RouteName", entity.RouteName),
            new SqlParameter("@DepartureLocation", entity.DepartureLocation),
            new SqlParameter("@ArrivalLocation", entity.ArrivalLocation),

            new SqlParameter("@RouteDescription",
                (object?)entity.RouteDescription ?? DBNull.Value),

            new SqlParameter("@DepartureTime",
                (object?)entity.DepartureTime ?? DBNull.Value),

            new SqlParameter("@ArrivalTime",
                (object?)entity.ArrivalTime ?? DBNull.Value),

            new SqlParameter("@IsActive", entity.IsActive),

            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };

        return _dbManager.ExecuteAsync(
            ServiceRouteQueries.Create,
            parameters,
            cancellationToken);
    }


    public Task<int> UpdateAsync(
        ServiceRoutes entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ServiceRouteId",
                SqlDbType.BigInt)
            {
                Value = entity.ServiceRouteId
            },

            new SqlParameter("@RouteName", entity.RouteName),
            new SqlParameter("@DepartureLocation", entity.DepartureLocation),
            new SqlParameter("@ArrivalLocation", entity.ArrivalLocation),

            new SqlParameter("@RouteDescription",
                (object?)entity.RouteDescription ?? DBNull.Value),

            new SqlParameter("@DepartureTime",
                (object?)entity.DepartureTime ?? DBNull.Value),

            new SqlParameter("@ArrivalTime",
                (object?)entity.ArrivalTime ?? DBNull.Value),

            new SqlParameter("@IsActive", entity.IsActive),

            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };

        return _dbManager.ExecuteAsync(
            ServiceRouteQueries.Update,
            parameters,
            cancellationToken);
    }


    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@ServiceRouteId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.ExecuteAsync(
            ServiceRouteQueries.Delete,
            parameters,
            cancellationToken);
    }
}