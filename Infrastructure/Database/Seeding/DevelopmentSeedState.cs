using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Seeding;

public static class DevelopmentSeedState
{
    public const string MarkerTableName = "__ImeceDevSeedState";

    public static async Task EnsureMarkerTableAsync(
        IDbExecutor executor,
        SqlConnection connection,
        int commandTimeoutSeconds,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await executor.ExecuteNonQueryAsync(
            connection,
            $"""
             IF OBJECT_ID(N'[dbo].[{MarkerTableName}]', N'U') IS NULL
             BEGIN
                 CREATE TABLE [dbo].[{MarkerTableName}]
                 (
                     [SeedVersion] NVARCHAR(64) NOT NULL PRIMARY KEY,
                     [AppliedAt] DATETIME2 NOT NULL
                 );
             END
             """,
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);
    }

    public static async Task<string?> GetAppliedVersionAsync(
        IDbExecutor executor,
        SqlConnection connection,
        int commandTimeoutSeconds,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureMarkerTableAsync(
            executor,
            connection,
            commandTimeoutSeconds,
            transaction,
            cancellationToken);

        var scalar = await executor.ExecuteScalarAsync(
            connection,
            $"""
             SELECT TOP (1) [SeedVersion]
             FROM [dbo].[{MarkerTableName}]
             ORDER BY [AppliedAt] DESC
             """,
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);

        return scalar as string;
    }

    public static async Task WriteAppliedVersionAsync(
        IDbExecutor executor,
        SqlConnection connection,
        string seedVersion,
        int commandTimeoutSeconds,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureMarkerTableAsync(
            executor,
            connection,
            commandTimeoutSeconds,
            transaction,
            cancellationToken);

        await executor.ExecuteNonQueryAsync(
            connection,
            $"""
             DELETE FROM [dbo].[{MarkerTableName}];

             INSERT INTO [dbo].[{MarkerTableName}] ([SeedVersion], [AppliedAt])
             VALUES (@SeedVersion, SYSUTCDATETIME());
             """,
            parameters: [new SqlParameter("@SeedVersion", seedVersion)],
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);
    }

    public static async Task ClearAsync(
        IDbExecutor executor,
        SqlConnection connection,
        int commandTimeoutSeconds,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        if (await TableExistsAsync(executor, connection, commandTimeoutSeconds, transaction, cancellationToken))
        {
            await executor.ExecuteNonQueryAsync(
                connection,
                $"DELETE FROM [dbo].[{MarkerTableName}];",
                transaction: transaction,
                commandTimeoutSeconds: commandTimeoutSeconds,
                cancellationToken: cancellationToken);
        }
    }

    private static async Task<bool> TableExistsAsync(
        IDbExecutor executor,
        SqlConnection connection,
        int commandTimeoutSeconds,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var exists = await executor.ExecuteScalarAsync(
            connection,
            $"""
             SELECT CASE WHEN OBJECT_ID(N'[dbo].[{MarkerTableName}]', N'U') IS NOT NULL THEN 1 ELSE 0 END
             """,
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);

        return Convert.ToInt32(exists) == 1;
    }
}
