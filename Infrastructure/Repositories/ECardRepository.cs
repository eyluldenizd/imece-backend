using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ECardRepository
{
    private readonly DbManager _dbManager;

    public ECardRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }


    public Task<List<ECards>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync(
            ECardQueries.GetAll,
            reader => new ECards
            {
                ECardId = reader.GetInt64(
                    reader.GetOrdinal("e_card_id")),

                Title = reader.GetString(
                    reader.GetOrdinal("title")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("description")),

                CardType = reader.IsDBNull(
                    reader.GetOrdinal("card_type"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("card_type")),

                ImageUrl = reader.IsDBNull(
                    reader.GetOrdinal("image_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("image_url")),

                RedirectUrl = reader.IsDBNull(
                    reader.GetOrdinal("redirect_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("redirect_url")),

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


    public Task<ECards?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@ECardId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QueryFirstOrDefaultAsync(
            ECardQueries.GetById,
            reader => new ECards
            {
                ECardId = reader.GetInt64(
                    reader.GetOrdinal("e_card_id")),

                Title = reader.GetString(
                    reader.GetOrdinal("title")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("description")),

                CardType = reader.IsDBNull(
                    reader.GetOrdinal("card_type"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("card_type")),

                ImageUrl = reader.IsDBNull(
                    reader.GetOrdinal("image_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("image_url")),

                RedirectUrl = reader.IsDBNull(
                    reader.GetOrdinal("redirect_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("redirect_url")),

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
        ECards entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@Title", entity.Title),

            new SqlParameter("@Description",
                (object?)entity.Description ?? DBNull.Value),

            new SqlParameter("@CardType",
                (object?)entity.CardType ?? DBNull.Value),

            new SqlParameter("@ImageUrl",
                (object?)entity.ImageUrl ?? DBNull.Value),

            new SqlParameter("@RedirectUrl",
                (object?)entity.RedirectUrl ?? DBNull.Value),

            new SqlParameter("@IsActive", entity.IsActive),

            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };


        return _dbManager.ExecuteAsync(
            ECardQueries.Create,
            parameters,
            cancellationToken);
    }


    public Task<int> UpdateAsync(
        ECards entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ECardId",
                SqlDbType.BigInt)
            {
                Value = entity.ECardId
            },

            new SqlParameter("@Title", entity.Title),

            new SqlParameter("@Description",
                (object?)entity.Description ?? DBNull.Value),

            new SqlParameter("@CardType",
                (object?)entity.CardType ?? DBNull.Value),

            new SqlParameter("@ImageUrl",
                (object?)entity.ImageUrl ?? DBNull.Value),

            new SqlParameter("@RedirectUrl",
                (object?)entity.RedirectUrl ?? DBNull.Value),

            new SqlParameter("@IsActive", entity.IsActive),

            new SqlParameter("@DisplayOrder",
                (object?)entity.DisplayOrder ?? DBNull.Value)
        };


        return _dbManager.ExecuteAsync(
            ECardQueries.Update,
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
                "@ECardId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };


        return _dbManager.ExecuteAsync(
            ECardQueries.Delete,
            parameters,
            cancellationToken);
    }
}