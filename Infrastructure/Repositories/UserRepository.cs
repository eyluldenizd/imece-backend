using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class UserRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserRepository(
        ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Users>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Users>(
            UserQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<List<Users>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Users>(
            UserQueries.GetActive,
            cancellationToken: cancellationToken);
    }

    public Task<Users?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@UserId",
                SqlDbType.Int)
            {
                Value = userId
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Users>(
            UserQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<Users?> GetByAzureObjectIdAsync(
        string azureObjectId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@AzureObjectId",
                SqlDbType.NVarChar,
                255)
            {
                Value = azureObjectId
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Users>(
            UserQueries.GetByAzureObjectId,
            parameters,
            cancellationToken);
    }

    public Task<Users?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@Email",
                SqlDbType.NVarChar,
                255)
            {
                Value = email
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Users>(
            UserQueries.GetByEmail,
            parameters,
            cancellationToken);
    }

    public Task<List<Users>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@SearchText",
                SqlDbType.NVarChar,
                255)
            {
                Value = $"%{searchText}%"
            }
        ];

        return _dataAccess.QueryAsync<Users>(
            UserQueries.Search,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(
        Users entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity);

        return _dataAccess.ExecuteScalarAsync<int>(
            UserQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        Users entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateUpdateParameters(entity);

        return _dataAccess.ExecuteAsync(
            UserQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateLastLoginAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@UserId",
                SqlDbType.Int)
            {
                Value = userId
            }
        ];

        return _dataAccess.ExecuteAsync(
            UserQueries.UpdateLastLogin,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateUpdateParameters(
        Users entity)
    {
        return
        [
            new SqlParameter(
                "@UserId",
                SqlDbType.Int)
            {
                Value = entity.UserId
            },

            .. CreateWriteParameters(entity)
        ];
    }

    private static SqlParameter[] CreateWriteParameters(
        Users entity)
    {
        return
        [
            new SqlParameter(
                "@AzureObjectId",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.AzureObjectId
            },

            new SqlParameter(
                "@Email",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Email
            },

            new SqlParameter(
                "@FullName",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.FullName
            },

            new SqlParameter(
                "@Title",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Title
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DepartmentId",
                SqlDbType.Int)
            {
                Value = entity.DepartmentId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@BranchId",
                SqlDbType.Int)
            {
                Value = entity.BranchId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@RoleId",
                SqlDbType.Int)
            {
                Value = entity.RoleId
            },

            new SqlParameter(
                "@BirthDate",
                SqlDbType.Date)
            {
                Value = entity.BirthDate is null
                    ? DBNull.Value
                    : entity.BirthDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@BirthMonth",
                SqlDbType.Int)
            {
                Value = entity.BirthMonth
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@BirthDay",
                SqlDbType.Int)
            {
                Value = entity.BirthDay
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@HireDate",
                SqlDbType.Date)
            {
                Value = entity.HireDate is null
                    ? DBNull.Value
                    : entity.HireDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@Phone",
                SqlDbType.NVarChar,
                30)
            {
                Value = entity.Phone
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@PhotoUrl",
                SqlDbType.NVarChar,
                500)
            {
                Value = entity.PhotoUrl
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@IsActive",
                SqlDbType.Bit)
            {
                Value = entity.IsActive
            }
        ];
    }
}
