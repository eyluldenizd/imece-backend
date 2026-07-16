using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class EmergencyNumberRepository
{
    private readonly DbManager _dbManager;

    public EmergencyNumberRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }


    public Task<List<EmergencyNumbers>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync(
            EmergencyNumberQueries.GetAll,
            reader => new EmergencyNumbers
            {
                EmergencyNumberId = reader.GetInt64(
                    reader.GetOrdinal("emergency_number_id")),

                Name = reader.GetString(
                    reader.GetOrdinal("name")),

                PhoneNumber = reader.GetString(
                    reader.GetOrdinal("phone_number")),

                Category = reader.GetString(
                    reader.GetOrdinal("category")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("description")),

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


    public Task<EmergencyNumbers?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@EmergencyNumberId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QueryFirstOrDefaultAsync(
            EmergencyNumberQueries.GetById,
            reader => new EmergencyNumbers
            {
                EmergencyNumberId = reader.GetInt64(
                    reader.GetOrdinal("emergency_number_id")),

                Name = reader.GetString(
                    reader.GetOrdinal("name")),

                PhoneNumber = reader.GetString(
                    reader.GetOrdinal("phone_number")),

                Category = reader.GetString(
                    reader.GetOrdinal("category")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("description")),

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
        EmergencyNumbers entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@Name", entity.Name),
            new SqlParameter("@PhoneNumber", entity.PhoneNumber),
            new SqlParameter("@Category", entity.Category),
            new SqlParameter("@Description",
                (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };

        return _dbManager.ExecuteAsync(
            EmergencyNumberQueries.Create,
            parameters,
            cancellationToken);
    }


    public Task<int> UpdateAsync(
        EmergencyNumbers entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@EmergencyNumberId",
                SqlDbType.BigInt)
            {
                Value = entity.EmergencyNumberId
            },

            new SqlParameter("@Name", entity.Name),
            new SqlParameter("@PhoneNumber", entity.PhoneNumber),
            new SqlParameter("@Category", entity.Category),
            new SqlParameter("@Description",
                (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };

        return _dbManager.ExecuteAsync(
            EmergencyNumberQueries.Update,
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
                "@EmergencyNumberId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.ExecuteAsync(
            EmergencyNumberQueries.Delete,
            parameters,
            cancellationToken);
    }
}