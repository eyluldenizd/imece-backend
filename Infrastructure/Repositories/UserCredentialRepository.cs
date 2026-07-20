using System.Data;
using Infrastructure.Authentication.Models;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class UserCredentialRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserCredentialRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<UserCredential?> FindByUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@NormalizedUsername", SqlDbType.NVarChar, 128)
            {
                Value = normalizedUsername
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<UserCredential>(
            UserCredentialQueries.FindByUsername,
            parameters,
            cancellationToken);
    }

    public Task UpdateLoginSuccessAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
        ];

        return _dataAccess.ExecuteAsync(
            UserCredentialQueries.UpdateLoginSuccess,
            parameters,
            cancellationToken);
    }

    public Task UpdateLoginFailureAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
        ];

        return _dataAccess.ExecuteAsync(
            UserCredentialQueries.UpdateLoginFailure,
            parameters,
            cancellationToken);
    }
}
