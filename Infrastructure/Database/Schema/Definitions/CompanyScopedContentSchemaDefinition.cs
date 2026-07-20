using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class CompanyScopedContentSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "CompanyScopedContent";

    private static CheckConstraintDefinition ScopeCheck(string tableName, string constraintName) =>
        new()
        {
            Name = constraintName,
            Expression =
                "((scope_type = N'Company' AND company_id IS NOT NULL) OR (scope_type = N'Global' AND company_id IS NULL))",
            PreApplySql = $"""
                    UPDATE [dbo].[{tableName}]
                    SET scope_type = CASE
                        WHEN company_id IS NULL THEN N'Global'
                        ELSE N'Company'
                    END
                    WHERE scope_type IS NULL
                       OR (scope_type = N'Company' AND company_id IS NULL)
                       OR (scope_type = N'Global' AND company_id IS NOT NULL);

                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns AS c
                        INNER JOIN sys.tables AS t ON t.object_id = c.object_id
                        INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                        WHERE s.name = N'dbo'
                          AND t.name = N'{tableName}'
                          AND c.name = N'company_id'
                          AND c.is_nullable = 0)
                    BEGIN
                        ALTER TABLE [dbo].[{tableName}] ALTER COLUMN [company_id] INT NULL;
                    END
                """
        };

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "media_folders",
            [
                Col("folder_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("scope_type", "NVARCHAR(16)", defaultExpression: "N'Company'"),
                Col("branch_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("branch_id", "INT", nullable: true),
                Col("department_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("department_id", "INT", nullable: true),
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
                Idx("IX_media_folders_scope_type", unique: false, "scope_type"),
                Idx("IX_media_folders_parent_folder_id", unique: false, "parent_folder_id")
            ],
            foreignKeys:
            [
                Fk("FK_media_folders_companies", "company_id", "companies", "company_id"),
                Fk("FK_media_folders_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_media_folders_departments", "department_id", "departments", "department_id"),
                Fk("FK_media_folders_parent", "parent_folder_id", "media_folders", "folder_id"),
                Fk("FK_media_folders_created_by", "created_by", "users", "user_id")
            ],
            checks:
            [
                ScopeCheck("media_folders", "CK_media_folders_scope")
            ]),

        Table(
            "media_files",
            [
                Col("media_file_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("scope_type", "NVARCHAR(16)", defaultExpression: "N'Company'"),
                Col("branch_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("branch_id", "INT", nullable: true),
                Col("department_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("department_id", "INT", nullable: true),
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
                Idx("IX_media_files_scope_type", unique: false, "scope_type"),
                Idx("IX_media_files_folder_id", unique: false, "folder_id"),
                Idx("UX_media_files_stored_file_name", unique: true, "stored_file_name")
            ],
            foreignKeys:
            [
                Fk("FK_media_files_companies", "company_id", "companies", "company_id"),
                Fk("FK_media_files_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_media_files_departments", "department_id", "departments", "department_id"),
                Fk("FK_media_files_folders", "folder_id", "media_folders", "folder_id"),
                Fk("FK_media_files_uploaded_by", "uploaded_by", "users", "user_id")
            ],
            checks:
            [
                ScopeCheck("media_files", "CK_media_files_scope")
            ]),

        Table(
            "announcements",
            [
                Col("announcement_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("scope_type", "NVARCHAR(16)", defaultExpression: "N'Company'"),
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
                Idx("IX_announcements_scope_type", unique: false, "scope_type"),
                Idx("IX_announcements_publish_start", unique: false, "publish_start")
            ],
            foreignKeys:
            [
                Fk("FK_announcements_companies", "company_id", "companies", "company_id")
            ],
            checks:
            [
                ScopeCheck("announcements", "CK_announcements_scope")
            ]),

        Table(
            "events",
            [
                Col("event_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("scope_type", "NVARCHAR(16)", defaultExpression: "N'Company'"),
                Col("branch_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("branch_id", "INT", nullable: true),
                Col("department_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("department_id", "INT", nullable: true),
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
                Idx("IX_events_scope_type", unique: false, "scope_type"),
                Idx("IX_events_start_datetime", unique: false, "start_datetime")
            ],
            foreignKeys:
            [
                Fk("FK_events_companies", "company_id", "companies", "company_id"),
                Fk("FK_events_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_events_departments", "department_id", "departments", "department_id"),
                Fk("FK_events_created_by", "created_by", "users", "user_id")
            ],
            checks:
            [
                ScopeCheck("events", "CK_events_scope")
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
            "meeting_rooms",
            [
                Col("meeting_room_id", "INT", identity: true, primaryKey: true),
                Col("company_id", "INT"),
                Col("branch_id", "INT", nullable: true),
                Col("department_id", "INT", nullable: true),
                Col("name", "NVARCHAR(256)"),
                Col("code", "NVARCHAR(64)"),
                Col("floor", "NVARCHAR(64)", nullable: true),
                Col("capacity", "INT"),
                Col("location_description", "NVARCHAR(512)", nullable: true),
                Col("features", "NVARCHAR(512)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_meeting_rooms_company_code", unique: true, "company_id", "code"),
                Idx("IX_meeting_rooms_company_id", unique: false, "company_id")
            ],
            foreignKeys:
            [
                Fk("FK_meeting_rooms_companies", "company_id", "companies", "company_id"),
                Fk("FK_meeting_rooms_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_meeting_rooms_departments", "department_id", "departments", "department_id")
            ]),

        Table(
            "reservations",
            [
                Col("reservation_id", "INT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("meeting_room_id", "INT", nullable: true),
                Col("room_name", "NVARCHAR(128)"),
                Col("organizer_user_id", "INT", nullable: true),
                Col("requester_user_id", "INT", nullable: true),
                Col("requester_name", "NVARCHAR(256)", nullable: true),
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
                Idx("IX_reservations_meeting_room_id", unique: false, "meeting_room_id"),
                Idx("IX_reservations_room_start", unique: false, "room_name", "start_time"),
                Idx("IX_reservations_organizer_user_id", unique: false, "organizer_user_id"),
                Idx("IX_reservations_requester_user_id", unique: false, "requester_user_id")
            ],
            foreignKeys:
            [
                Fk("FK_reservations_companies", "company_id", "companies", "company_id"),
                Fk("FK_reservations_meeting_rooms", "meeting_room_id", "meeting_rooms", "meeting_room_id"),
                Fk("FK_reservations_organizer", "organizer_user_id", "users", "user_id"),
                Fk("FK_reservations_requester", "requester_user_id", "users", "user_id")
            ]),

        Table(
            "social_activities",
            [
                Col("social_activity_id", "BIGINT", identity: true, primaryKey: true),
                Col("title", "NVARCHAR(512)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("activity_type", "NVARCHAR(64)"),
                Col("location", "NVARCHAR(256)", nullable: true),
                Col("start_at", "DATETIME2"),
                Col("end_at", "DATETIME2"),
                Col("image_url", "NVARCHAR(1024)", nullable: true),
                Col("company_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("company_id", "INT", nullable: true),
                Col("branch_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("branch_id", "INT", nullable: true),
                Col("department_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
                Col("department_id", "INT", nullable: true),
                Col("status", "NVARCHAR(32)", defaultExpression: "N'Draft'"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_by", "INT"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_social_activities_company_id", unique: false, "company_id"),
                Idx("IX_social_activities_status", unique: false, "status"),
                Idx("IX_social_activities_start_at", unique: false, "start_at")
            ],
            foreignKeys:
            [
                Fk("FK_social_activities_companies", "company_id", "companies", "company_id"),
                Fk("FK_social_activities_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_social_activities_departments", "department_id", "departments", "department_id"),
                Fk("FK_social_activities_created_by", "created_by", "users", "user_id")
            ])
    ];
}
