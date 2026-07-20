using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class MediaFolderDetails
{
    [DbManager.DbColumn("folder_id")]
    public long FolderId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("parent_folder_id")]
    public long? ParentFolderId { get; set; }

    [DbManager.DbColumn("parent_folder_name")]
    public string? ParentFolderName { get; set; }

    [DbManager.DbColumn("folder_name")]
    public string FolderName { get; set; } = string.Empty;

    [DbManager.DbColumn("folder_type")]
    public string FolderType { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("event_id")]
    public long? EventId { get; set; }

    [DbManager.DbColumn("event_title")]
    public string? EventTitle { get; set; }

    [DbManager.DbColumn("cover_media_file_id")]
    public long? CoverMediaFileId { get; set; }

    [DbManager.DbColumn("cover_media_path")]
    public string? CoverMediaPath { get; set; }

    [DbManager.DbColumn("is_public")]
    public bool IsPublic { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_by")]
    public int CreatedBy { get; set; }

    [DbManager.DbColumn("created_by_full_name")]
    public string CreatedByFullName { get; set; } = string.Empty;

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public sealed class MediaFolderRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public MediaFolderRepository(
        ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<MediaFolderDetails>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<MediaFolderDetails>(
            MediaFolderQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<List<MediaFolderDetails>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<MediaFolderDetails>(
            MediaFolderQueries.GetActive,
            cancellationToken: cancellationToken);
    }

    public Task<MediaFolderDetails?> GetByIdAsync(
        long folderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = folderId
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<MediaFolderDetails>(
            MediaFolderQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFolderDetails>> GetByCompanyAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@CompanyId",
                SqlDbType.Int)
            {
                Value = companyId
            }
        ];

        return _dataAccess.QueryAsync<MediaFolderDetails>(
            MediaFolderQueries.GetByCompany,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFolderDetails>> GetChildrenAsync(
        long parentFolderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@ParentFolderId",
                SqlDbType.BigInt)
            {
                Value = parentFolderId
            }
        ];

        return _dataAccess.QueryAsync<MediaFolderDetails>(
            MediaFolderQueries.GetChildren,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(
        MediaFolders entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity);

        return _dataAccess.ExecuteScalarAsync<long>(
            MediaFolderQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        MediaFolders entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity)
            .ToList();

        parameters.Add(
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = entity.FolderId
            });

        return _dataAccess.ExecuteAsync(
            MediaFolderQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        long folderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = folderId
            }
        ];

        return _dataAccess.ExecuteAsync(
            MediaFolderQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(
        long folderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = folderId
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            MediaFolderQueries.ExistsById,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public async Task<bool> HasActiveChildrenAsync(
        long folderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = folderId
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            MediaFolderQueries.HasActiveChildren,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public async Task<bool> HasActiveFilesAsync(
        long folderId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = folderId
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            MediaFolderQueries.HasActiveFiles,
            parameters,
            cancellationToken);

        return count > 0;
    }

    private static SqlParameter[] CreateWriteParameters(
        MediaFolders entity)
    {
        return
        [
            new SqlParameter(
                "@CompanyId",
                SqlDbType.Int)
            {
                Value = entity.CompanyId
            },

            new SqlParameter(
                "@ParentFolderId",
                SqlDbType.BigInt)
            {
                Value = entity.ParentFolderId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@FolderName",
                SqlDbType.NVarChar,
                200)
            {
                Value = entity.FolderName
            },

            new SqlParameter(
                "@FolderType",
                SqlDbType.NVarChar,
                30)
            {
                Value = entity.FolderType
            },

            new SqlParameter(
                "@Description",
                SqlDbType.NVarChar,
                1000)
            {
                Value = entity.Description
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = entity.EventId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@CoverMediaFileId",
                SqlDbType.BigInt)
            {
                Value = entity.CoverMediaFileId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@IsPublic",
                SqlDbType.Bit)
            {
                Value = entity.IsPublic
            },

            new SqlParameter(
                "@IsActive",
                SqlDbType.Bit)
            {
                Value = entity.IsActive
            },

            new SqlParameter(
                "@CreatedBy",
                SqlDbType.Int)
            {
                Value = entity.CreatedBy
            }
        ];
    }
}