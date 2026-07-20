using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

/// <summary>
/// Şirket kapsamlı içerik tabloları. company_id mevcut sorgularla uyumlu olacak
/// şekilde media_* için zorunlu; diğerlerinde backfill için nullable başlar.
/// announcements.author_user_id kasıtlı olarak YOKTUR.
/// </summary>
public sealed class CompanyScopedContentSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "CompanyScopedContent";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "media_folders",
            [
                Col("folder_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT"),
                Col("parent_folder_id", "BIGINT", nullable: true),
                Col("folder_name", "NVARCHAR(256)"),
                Col("folder_type", "NVARCHAR(64)"),
                Col("description", "NVARCHAR(1024)", nullable: true),
                Col("event_id", "BIGINT", nullable: true),
                Col("cover_media_file_id", "BIGINT", nullable: true),
                Col("is_public", "BIT", defaultExpression: "0"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_by", "INT"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_media_folders_company_id", unique: false, "company_id"),
                Idx("IX_media_folders_parent_folder_id", unique: false, "parent_folder_id")
            ],
            foreignKeys:
            [
                Fk("FK_media_folders_companies", "company_id", "companies", "company_id"),
                Fk("FK_media_folders_parent", "parent_folder_id", "media_folders", "folder_id"),
                Fk("FK_media_folders_created_by", "created_by", "users", "user_id")
            ]),

        Table(
            "media_files",
            [
                Col("media_file_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT"),
                Col("folder_id", "BIGINT", nullable: true),
                Col("media_type", "NVARCHAR(32)"),
                Col("title", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(1024)", nullable: true),
                Col("original_file_name", "NVARCHAR(512)"),
                Col("stored_file_name", "NVARCHAR(512)"),
                Col("file_extension", "NVARCHAR(32)"),
                Col("content_type", "NVARCHAR(128)"),
                Col("relative_path", "NVARCHAR(1024)"),
                Col("thumbnail_path", "NVARCHAR(1024)", nullable: true),
                Col("file_size_bytes", "BIGINT"),
                Col("duration_seconds", "INT", nullable: true),
                Col("document_number", "NVARCHAR(128)", nullable: true),
                Col("document_version", "NVARCHAR(64)", nullable: true),
                Col("effective_date", "DATE", nullable: true),
                Col("expiry_date", "DATE", nullable: true),
                Col("sort_order", "INT", defaultExpression: "0"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("uploaded_by", "INT"),
                Col("uploaded_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_media_files_company_id", unique: false, "company_id"),
                Idx("IX_media_files_folder_id", unique: false, "folder_id"),
                Idx("UX_media_files_stored_file_name", unique: true, "stored_file_name")
            ],
            foreignKeys:
            [
                Fk("FK_media_files_companies", "company_id", "companies", "company_id"),
                Fk("FK_media_files_folders", "folder_id", "media_folders", "folder_id"),
                Fk("FK_media_files_uploaded_by", "uploaded_by", "users", "user_id")
            ]),

        Table(
            "announcements",
            [
                Col("announcement_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("title", "NVARCHAR(512)"),
                Col("content", "NVARCHAR(MAX)"),
                Col("cover_image_url", "NVARCHAR(1024)", nullable: true),
                Col("is_pinned", "BIT", defaultExpression: "0"),
                Col("publish_start", "DATETIME2"),
                Col("publish_end", "DATETIME2", nullable: true),
                Col("view_count", "INT", defaultExpression: "0"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_announcements_company_id", unique: false, "company_id"),
                Idx("IX_announcements_publish_start", unique: false, "publish_start")
            ],
            foreignKeys:
            [
                Fk("FK_announcements_companies", "company_id", "companies", "company_id")
            ]),

        Table(
            "events",
            [
                Col("event_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("title", "NVARCHAR(512)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("event_type", "NVARCHAR(64)", nullable: true),
                Col("location", "NVARCHAR(256)", nullable: true),
                Col("cover_image_url", "NVARCHAR(1024)", nullable: true),
                Col("start_datetime", "DATETIME2"),
                Col("end_datetime", "DATETIME2"),
                Col("is_all_day", "BIT", defaultExpression: "0"),
                Col("created_by", "INT"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_events_company_id", unique: false, "company_id"),
                Idx("IX_events_start_datetime", unique: false, "start_datetime")
            ],
            foreignKeys:
            [
                Fk("FK_events_companies", "company_id", "companies", "company_id"),
                Fk("FK_events_created_by", "created_by", "users", "user_id")
            ]),

        Table(
            "weekly_menu_entries",
            [
                Col("entry_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("week_id", "INT"),
                Col("dish_id", "INT"),
                Col("branch_id", "INT"),
                Col("menu_date", "DATE"),
                Col("meal_type", "NVARCHAR(32)"),
                Col("sort_order", "SMALLINT", defaultExpression: "0"),
                Col("created_by", "INT", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_weekly_menu_entries_company_id", unique: false, "company_id"),
                Idx("IX_weekly_menu_entries_menu_date", unique: false, "menu_date"),
                Idx("IX_weekly_menu_entries_week_id", unique: false, "week_id")
            ],
            foreignKeys:
            [
                Fk("FK_weekly_menu_entries_companies", "company_id", "companies", "company_id"),
                Fk("FK_weekly_menu_entries_weeks", "week_id", "weeks", "week_id"),
                Fk("FK_weekly_menu_entries_dishes", "dish_id", "dishes", "dish_id"),
                Fk("FK_weekly_menu_entries_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_weekly_menu_entries_created_by", "created_by", "users", "user_id")
            ]),

        Table(
            "reservations",
            [
                Col("reservation_id", "INT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("room_name", "NVARCHAR(128)"),
                Col("organizer_user_id", "INT"),
                Col("title", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("start_time", "DATETIME2"),
                Col("end_time", "DATETIME2"),
                Col("status", "NVARCHAR(32)"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_reservations_company_id", unique: false, "company_id"),
                Idx("IX_reservations_room_start", unique: false, "room_name", "start_time"),
                Idx("IX_reservations_organizer_user_id", unique: false, "organizer_user_id")
            ],
            foreignKeys:
            [
                Fk("FK_reservations_companies", "company_id", "companies", "company_id"),
                Fk("FK_reservations_organizer", "organizer_user_id", "users", "user_id")
            ])
    ];
}
