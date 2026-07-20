namespace Infrastructure.Repositories.Queries;

public static class MediaFileQueries
{
    private const string SelectColumns = """
        SELECT
            mf.media_file_id,
            mf.company_id,
            mf.scope_type,
            mf.branch_scope,
            mf.branch_id,
            mf.department_scope,
            mf.department_id,
            mf.folder_id,
            folder.folder_name,
            mf.media_type,
            mf.title,
            mf.description,
            mf.original_file_name,
            mf.stored_file_name,
            mf.file_extension,
            mf.content_type,
            mf.relative_path,
            mf.thumbnail_path,
            mf.file_size_bytes,
            mf.duration_seconds,
            mf.document_number,
            mf.document_version,
            mf.effective_date,
            mf.expiry_date,
            mf.sort_order,
            mf.is_active,
            mf.uploaded_by,
            u.full_name AS uploaded_by_full_name,
            mf.uploaded_at,
            mf.updated_at
        FROM media_files AS mf
        LEFT JOIN media_folders AS folder
            ON folder.folder_id = mf.folder_id
        INNER JOIN users AS u
            ON u.user_id = mf.uploaded_by

        """;

    public static readonly string GetAll =
        SelectColumns +
        $"""
        WHERE {CompanyScopeSql.MediaFileListFilter}
        ORDER BY mf.uploaded_at DESC;
        """;

    public static readonly string GetActive =
        SelectColumns +
        $"""
        WHERE mf.is_active = 1
          AND {CompanyScopeSql.MediaFileListFilter}
        ORDER BY mf.uploaded_at DESC;
        """;

    public static readonly string GetById =
        SelectColumns +
        """
        WHERE mf.media_file_id = @MediaFileId;
        """;

    public static readonly string GetByCompany =
        SelectColumns +
        """
        WHERE mf.company_id = @CompanyId
          AND mf.is_active = 1
        ORDER BY mf.uploaded_at DESC;
        """;

    public static readonly string GetByFolder =
        SelectColumns +
        """
        WHERE mf.folder_id = @FolderId
          AND mf.is_active = 1
        ORDER BY
            mf.sort_order ASC,
            mf.uploaded_at DESC;
        """;

    public static readonly string GetByMediaType =
        SelectColumns +
        """
        WHERE mf.media_type = @MediaType
          AND mf.is_active = 1
        ORDER BY mf.uploaded_at DESC;
        """;

    public static readonly string Search =
        SelectColumns +
        """
        WHERE mf.is_active = 1
          AND
          (
              mf.title LIKE @SearchText
              OR mf.original_file_name LIKE @SearchText
              OR mf.description LIKE @SearchText
          )
        ORDER BY mf.uploaded_at DESC;
        """;

    public const string Create = """
        INSERT INTO media_files
        (
            company_id,
            scope_type,
            branch_scope,
            branch_id,
            department_scope,
            department_id,
            folder_id,
            media_type,
            title,
            description,
            original_file_name,
            stored_file_name,
            file_extension,
            content_type,
            relative_path,
            thumbnail_path,
            file_size_bytes,
            duration_seconds,
            document_number,
            document_version,
            effective_date,
            expiry_date,
            sort_order,
            is_active,
            uploaded_by
        )
        OUTPUT INSERTED.media_file_id
        VALUES
        (
            @CompanyId,
            @ScopeType,
            @BranchScope,
            @BranchId,
            @DepartmentScope,
            @DepartmentId,
            @FolderId,
            @MediaType,
            @Title,
            @Description,
            @OriginalFileName,
            @StoredFileName,
            @FileExtension,
            @ContentType,
            @RelativePath,
            @ThumbnailPath,
            @FileSizeBytes,
            @DurationSeconds,
            @DocumentNumber,
            @DocumentVersion,
            @EffectiveDate,
            @ExpiryDate,
            @SortOrder,
            @IsActive,
            @UploadedBy
        );
        """;

    public const string Update = """
        UPDATE media_files
        SET
            folder_id = @FolderId,
            title = @Title,
            description = @Description,
            thumbnail_path = @ThumbnailPath,
            duration_seconds = @DurationSeconds,
            document_number = @DocumentNumber,
            document_version = @DocumentVersion,
            effective_date = @EffectiveDate,
            expiry_date = @ExpiryDate,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSDATETIME()
        WHERE media_file_id = @MediaFileId;
        """;

    public const string SoftDelete = """
        UPDATE media_files
        SET
            is_active = 0,
            updated_at = SYSDATETIME()
        WHERE media_file_id = @MediaFileId
          AND is_active = 1;
        """;

    public const string ExistsByStoredFileName = """
        SELECT COUNT(1)
        FROM media_files
        WHERE stored_file_name = @StoredFileName;
        """;
}