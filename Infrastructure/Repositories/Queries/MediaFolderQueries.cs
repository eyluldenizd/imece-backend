namespace Infrastructure.Repositories.Queries;

public static class MediaFolderQueries
{
    private const string SelectColumns = """
        SELECT
            mf.folder_id,
            mf.company_id,
            mf.parent_folder_id,
            parent.folder_name AS parent_folder_name,
            mf.folder_name,
            mf.folder_type,
            mf.description,
            mf.event_id,
            e.title AS event_title,
            mf.cover_media_file_id,
            cover.relative_path AS cover_media_path,
            mf.is_public,
            mf.is_active,
            mf.created_by,
            u.full_name AS created_by_full_name,
            mf.created_at,
            mf.updated_at
        FROM media_folders AS mf
        LEFT JOIN media_folders AS parent
            ON parent.folder_id = mf.parent_folder_id
        LEFT JOIN events AS e
            ON e.event_id = mf.event_id
        LEFT JOIN media_files AS cover
            ON cover.media_file_id = mf.cover_media_file_id
        INNER JOIN users AS u
            ON u.user_id = mf.created_by

        """;

    public static readonly string GetAll =
        SelectColumns +
        """
        ORDER BY mf.folder_name ASC;
        """;

    public static readonly string GetActive =
        SelectColumns +
        """
        WHERE mf.is_active = 1
        ORDER BY mf.folder_name ASC;
        """;

    public static readonly string GetById =
        SelectColumns +
        """
        WHERE mf.folder_id = @FolderId;
        """;

    public static readonly string GetByCompany =
        SelectColumns +
        """
        WHERE mf.company_id = @CompanyId
          AND mf.is_active = 1
        ORDER BY mf.folder_name ASC;
        """;

    public static readonly string GetChildren =
        SelectColumns +
        """
        WHERE mf.parent_folder_id = @ParentFolderId
          AND mf.is_active = 1
        ORDER BY mf.folder_name ASC;
        """;

    public const string Create = """
        INSERT INTO media_folders
        (
            company_id,
            parent_folder_id,
            folder_name,
            folder_type,
            description,
            event_id,
            cover_media_file_id,
            is_public,
            is_active,
            created_by
        )
        OUTPUT INSERTED.folder_id
        VALUES
        (
            @CompanyId,
            @ParentFolderId,
            @FolderName,
            @FolderType,
            @Description,
            @EventId,
            @CoverMediaFileId,
            @IsPublic,
            @IsActive,
            @CreatedBy
        );
        """;

    public const string Update = """
        UPDATE media_folders
        SET
            parent_folder_id = @ParentFolderId,
            folder_name = @FolderName,
            folder_type = @FolderType,
            description = @Description,
            event_id = @EventId,
            cover_media_file_id = @CoverMediaFileId,
            is_public = @IsPublic,
            is_active = @IsActive,
            updated_at = SYSDATETIME()
        WHERE folder_id = @FolderId;
        """;

    public const string SoftDelete = """
        UPDATE media_folders
        SET
            is_active = 0,
            updated_at = SYSDATETIME()
        WHERE folder_id = @FolderId
          AND is_active = 1;
        """;

    public const string ExistsById = """
        SELECT COUNT(1)
        FROM media_folders
        WHERE folder_id = @FolderId;
        """;

    public const string HasActiveChildren = """
        SELECT COUNT(1)
        FROM media_folders
        WHERE parent_folder_id = @FolderId
          AND is_active = 1;
        """;

    public const string HasActiveFiles = """
        SELECT COUNT(1)
        FROM media_files
        WHERE folder_id = @FolderId
          AND is_active = 1;
        """;
}