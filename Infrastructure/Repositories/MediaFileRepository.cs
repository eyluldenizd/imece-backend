using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class MediaFileDetails
{
    [DbManager.DbColumn("media_file_id")]
    public long MediaFileId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("folder_id")]
    public long? FolderId { get; set; }

    [DbManager.DbColumn("folder_name")]
    public string? FolderName { get; set; }

    [DbManager.DbColumn("media_type")]
    public string MediaType { get; set; } = string.Empty;

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = string.Empty;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("original_file_name")]
    public string OriginalFileName { get; set; } = string.Empty;

    [DbManager.DbColumn("stored_file_name")]
    public string StoredFileName { get; set; } = string.Empty;

    [DbManager.DbColumn("file_extension")]
    public string FileExtension { get; set; } = string.Empty;

    [DbManager.DbColumn("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [DbManager.DbColumn("relative_path")]
    public string RelativePath { get; set; } = string.Empty;

    [DbManager.DbColumn("thumbnail_path")]
    public string? ThumbnailPath { get; set; }

    [DbManager.DbColumn("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [DbManager.DbColumn("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [DbManager.DbColumn("document_number")]
    public string? DocumentNumber { get; set; }

    [DbManager.DbColumn("document_version")]
    public string? DocumentVersion { get; set; }

    [DbManager.DbColumn("effective_date")]
    public DateOnly? EffectiveDate { get; set; }

    [DbManager.DbColumn("expiry_date")]
    public DateOnly? ExpiryDate { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("uploaded_by")]
    public int UploadedBy { get; set; }

    [DbManager.DbColumn("uploaded_by_full_name")]
    public string UploadedByFullName { get; set; } = string.Empty;

    [DbManager.DbColumn("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public sealed class MediaFileRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public MediaFileRepository(
        ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<MediaFileDetails>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<List<MediaFileDetails>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.GetActive,
            cancellationToken: cancellationToken);
    }

    public Task<MediaFileDetails?> GetByIdAsync(
        long mediaFileId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@MediaFileId",
                SqlDbType.BigInt)
            {
                Value = mediaFileId
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<MediaFileDetails>(
            MediaFileQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFileDetails>> GetByCompanyAsync(
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

        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.GetByCompany,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFileDetails>> GetByFolderAsync(
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

        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.GetByFolder,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFileDetails>> GetByMediaTypeAsync(
        string mediaType,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@MediaType",
                SqlDbType.NVarChar,
                30)
            {
                Value = mediaType
            }
        ];

        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.GetByMediaType,
            parameters,
            cancellationToken);
    }

    public Task<List<MediaFileDetails>> SearchAsync(
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

        return _dataAccess.QueryAsync<MediaFileDetails>(
            MediaFileQueries.Search,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(
        MediaFiles entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity);

        return _dataAccess.ExecuteScalarAsync<long>(
            MediaFileQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        MediaFiles entity,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@MediaFileId",
                SqlDbType.BigInt)
            {
                Value = entity.MediaFileId
            },

            new SqlParameter(
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = entity.FolderId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@Title",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Title
            },

            new SqlParameter(
                "@Description",
                SqlDbType.NVarChar,
                2000)
            {
                Value = entity.Description
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@ThumbnailPath",
                SqlDbType.NVarChar,
                1000)
            {
                Value = entity.ThumbnailPath
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DurationSeconds",
                SqlDbType.Int)
            {
                Value = entity.DurationSeconds
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DocumentNumber",
                SqlDbType.NVarChar,
                100)
            {
                Value = entity.DocumentNumber
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DocumentVersion",
                SqlDbType.NVarChar,
                50)
            {
                Value = entity.DocumentVersion
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@EffectiveDate",
                SqlDbType.Date)
            {
                Value = entity.EffectiveDate is null
                    ? DBNull.Value
                    : entity.EffectiveDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@ExpiryDate",
                SqlDbType.Date)
            {
                Value = entity.ExpiryDate is null
                    ? DBNull.Value
                    : entity.ExpiryDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@SortOrder",
                SqlDbType.Int)
            {
                Value = entity.SortOrder
            },

            new SqlParameter(
                "@IsActive",
                SqlDbType.Bit)
            {
                Value = entity.IsActive
            }
        ];

        return _dataAccess.ExecuteAsync(
            MediaFileQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        long mediaFileId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@MediaFileId",
                SqlDbType.BigInt)
            {
                Value = mediaFileId
            }
        ];

        return _dataAccess.ExecuteAsync(
            MediaFileQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByStoredFileNameAsync(
        string storedFileName,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@StoredFileName",
                SqlDbType.NVarChar,
                255)
            {
                Value = storedFileName
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            MediaFileQueries.ExistsByStoredFileName,
            parameters,
            cancellationToken);

        return count > 0;
    }

    private static SqlParameter[] CreateWriteParameters(
        MediaFiles entity)
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
                "@FolderId",
                SqlDbType.BigInt)
            {
                Value = entity.FolderId
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@MediaType",
                SqlDbType.NVarChar,
                30)
            {
                Value = entity.MediaType
            },

            new SqlParameter(
                "@Title",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Title
            },

            new SqlParameter(
                "@Description",
                SqlDbType.NVarChar,
                2000)
            {
                Value = entity.Description
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@OriginalFileName",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.OriginalFileName
            },

            new SqlParameter(
                "@StoredFileName",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.StoredFileName
            },

            new SqlParameter(
                "@FileExtension",
                SqlDbType.NVarChar,
                20)
            {
                Value = entity.FileExtension
            },

            new SqlParameter(
                "@ContentType",
                SqlDbType.NVarChar,
                150)
            {
                Value = entity.ContentType
            },

            new SqlParameter(
                "@RelativePath",
                SqlDbType.NVarChar,
                1000)
            {
                Value = entity.RelativePath
            },

            new SqlParameter(
                "@ThumbnailPath",
                SqlDbType.NVarChar,
                1000)
            {
                Value = entity.ThumbnailPath
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@FileSizeBytes",
                SqlDbType.BigInt)
            {
                Value = entity.FileSizeBytes
            },

            new SqlParameter(
                "@DurationSeconds",
                SqlDbType.Int)
            {
                Value = entity.DurationSeconds
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DocumentNumber",
                SqlDbType.NVarChar,
                100)
            {
                Value = entity.DocumentNumber
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@DocumentVersion",
                SqlDbType.NVarChar,
                50)
            {
                Value = entity.DocumentVersion
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@EffectiveDate",
                SqlDbType.Date)
            {
                Value = entity.EffectiveDate is null
                    ? DBNull.Value
                    : entity.EffectiveDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@ExpiryDate",
                SqlDbType.Date)
            {
                Value = entity.ExpiryDate is null
                    ? DBNull.Value
                    : entity.ExpiryDate.Value.ToDateTime(
                        TimeOnly.MinValue)
            },

            new SqlParameter(
                "@SortOrder",
                SqlDbType.Int)
            {
                Value = entity.SortOrder
            },

            new SqlParameter(
                "@IsActive",
                SqlDbType.Bit)
            {
                Value = entity.IsActive
            },

            new SqlParameter(
                "@UploadedBy",
                SqlDbType.Int)
            {
                Value = entity.UploadedBy
            }
        ];
    }
}