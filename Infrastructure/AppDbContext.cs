using System;
using System.Collections.Generic;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcements> Announcements { get; set; }

    public virtual DbSet<Branches> Branches { get; set; }

    public virtual DbSet<ContentTargets> ContentTargets { get; set; }

    public virtual DbSet<Departments> Departments { get; set; }

    public virtual DbSet<Dishes> Dishes { get; set; }

    public virtual DbSet<EventParticipants> EventParticipants { get; set; }

    public virtual DbSet<Events> Events { get; set; }

    public virtual DbSet<PhotoAlbums> PhotoAlbums { get; set; }

    public virtual DbSet<Photos> Photos { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<UserRoles> UserRoles { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<VideoAlbums> VideoAlbums { get; set; }

    public virtual DbSet<Videos> Videos { get; set; }

    public virtual DbSet<WeeklyMenuEntries> WeeklyMenuEntries { get; set; }

    public virtual DbSet<Weeks> Weeks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcements>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__announce__C640A82D0B1EB8A3");

            entity.ToTable("announcements", tb => tb.HasTrigger("trg_announcements_updated_at"));

            entity.HasIndex(e => new { e.PublishStart, e.PublishEnd }, "idx_ann_publish");

            entity.Property(e => e.AnnouncementId).HasColumnName("announcement_id");
            entity.Property(e => e.AuthorUserId).HasColumnName("author_user_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_image_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPinned).HasColumnName("is_pinned");
            entity.Property(e => e.PublishEnd).HasColumnName("publish_end");
            entity.Property(e => e.PublishStart)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("publish_start");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ann_author");
        });

        modelBuilder.Entity<Branches>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__branches__E55E37DE696870E4");

            entity.ToTable("branches");

            entity.HasIndex(e => e.BranchCode, "uq_branches_code").IsUnique();

            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BranchCode)
                .HasMaxLength(20)
                .HasColumnName("branch_code");
            entity.Property(e => e.BranchName)
                .HasMaxLength(150)
                .HasColumnName("branch_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
        });

        modelBuilder.Entity<ContentTargets>(entity =>
        {
            entity.HasKey(e => e.TargetRowId).HasName("PK__content___C3350EE5B1DA341C");

            entity.ToTable("content_targets");

            entity.HasIndex(e => new { e.ContentType, e.ContentId }, "idx_content");

            entity.HasIndex(e => new { e.ContentType, e.ContentId, e.TargetType, e.TargetId }, "uq_target").IsUnique();

            entity.Property(e => e.TargetRowId).HasColumnName("target_row_id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(30)
                .HasColumnName("content_type");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .HasColumnName("target_type");
        });

        modelBuilder.Entity<Departments>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__departme__C223242213208EA1");

            entity.ToTable("departments");

            entity.HasIndex(e => e.DepartmentCode, "uq_dept_code").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(20)
                .HasColumnName("department_code");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(150)
                .HasColumnName("department_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ParentDepartmentId).HasColumnName("parent_department_id");

            entity.HasOne(d => d.ParentDepartment).WithMany(p => p.InverseParentDepartment)
                .HasForeignKey(d => d.ParentDepartmentId)
                .HasConstraintName("fk_dept_parent");
        });

        modelBuilder.Entity<Dishes>(entity =>
        {
            entity.HasKey(e => e.DishId).HasName("PK__dishes__9F2B4CF9D2416D20");

            entity.ToTable("dishes");

            entity.HasIndex(e => new { e.DishName, e.Category }, "uq_dish_name_category").IsUnique();

            entity.Property(e => e.DishId).HasColumnName("dish_id");
            entity.Property(e => e.Calorie).HasColumnName("calorie");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DishName)
                .HasMaxLength(150)
                .HasColumnName("dish_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<EventParticipants>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.UserId }).HasName("PK__event_pa__C8EB145751A635F5");

            entity.ToTable("event_participants");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("registered_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("katiliyor")
                .HasColumnName("status");

            entity.HasOne(d => d.Event).WithMany(p => p.EventParticipants)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_ep_event");

            entity.HasOne(d => d.User).WithMany(p => p.EventParticipants)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ep_user");
        });

        modelBuilder.Entity<Events>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__events__2370F727F1C3514A");

            entity.ToTable("events");

            entity.HasIndex(e => new { e.StartDatetime, e.EndDatetime }, "idx_event_dates");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(255)
                .HasColumnName("cover_image_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDatetime).HasColumnName("end_datetime");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IsAllDay).HasColumnName("is_all_day");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.StartDatetime).HasColumnName("start_datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_event_creator");
        });

        modelBuilder.Entity<PhotoAlbums>(entity =>
        {
            entity.HasKey(e => e.AlbumId).HasName("PK__photo_al__B0E1DDB26610B7DC");

            entity.ToTable("photo_albums");

            entity.Property(e => e.AlbumId).HasColumnName("album_id");
            entity.Property(e => e.CoverPhotoId).HasColumnName("cover_photo_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CoverPhoto).WithMany(p => p.PhotoAlbums)
                .HasForeignKey(d => d.CoverPhotoId)
                .HasConstraintName("fk_album_cover");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PhotoAlbums)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_album_creator");

            entity.HasOne(d => d.Event).WithMany(p => p.PhotoAlbums)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_album_event");
        });

        modelBuilder.Entity<Photos>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__photos__CB48C83D63024FB2");

            entity.ToTable("photos");

            entity.Property(e => e.PhotoId).HasColumnName("photo_id");
            entity.Property(e => e.AlbumId).HasColumnName("album_id");
            entity.Property(e => e.Caption)
                .HasMaxLength(255)
                .HasColumnName("caption");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(255)
                .HasColumnName("file_url");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(255)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");

            entity.HasOne(d => d.Album).WithMany(p => p.Photos)
                .HasForeignKey(d => d.AlbumId)
                .HasConstraintName("fk_photo_album");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Photos)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_photo_uploader");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC318D74A6");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "uq_roles_name").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<UserRoles>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__user_rol__6EDEA153320B1315");

            entity.ToTable("user_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("assigned_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_roles_role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_roles_user");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F7F958559");

            entity.ToTable("users", tb => tb.HasTrigger("trg_users_updated_at"));

            entity.HasIndex(e => new { e.BirthMonth, e.BirthDay }, "idx_users_birth_month_day");

            entity.HasIndex(e => e.BranchId, "idx_users_branch");

            entity.HasIndex(e => e.DepartmentId, "idx_users_department");

            entity.HasIndex(e => e.AzureObjectId, "uq_users_azure").IsUnique();

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AzureObjectId)
                .HasMaxLength(64)
                .HasColumnName("azure_object_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.BirthDay)
                .HasComputedColumnSql("(datepart(day,[birth_date]))", true)
                .HasColumnName("birth_day");
            entity.Property(e => e.BirthMonth)
                .HasComputedColumnSql("(datepart(month,[birth_date]))", true)
                .HasColumnName("birth_month");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(255)
                .HasColumnName("photo_url");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("fk_users_branch");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_users_department");
        });

        modelBuilder.Entity<VideoAlbums>(entity =>
        {
            entity.HasKey(e => e.VideoAlbumId).HasName("PK__video_al__BF09807564B38067");

            entity.ToTable("video_albums");

            entity.Property(e => e.VideoAlbumId).HasColumnName("video_album_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VideoAlbums)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_valbum_creator");
        });

        modelBuilder.Entity<Videos>(entity =>
        {
            entity.HasKey(e => e.VideoId).HasName("PK__videos__E8F11E10B10CB9A1");

            entity.ToTable("videos");

            entity.HasIndex(e => e.VideoAlbumId, "idx_videos_album");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(255)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.VideoAlbumId).HasColumnName("video_album_id");
            entity.Property(e => e.VideoSource)
                .HasMaxLength(20)
                .HasDefaultValue("upload")
                .HasColumnName("video_source");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(255)
                .HasColumnName("video_url");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Videos)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_video_uploader");

            entity.HasOne(d => d.VideoAlbum).WithMany(p => p.Videos)
                .HasForeignKey(d => d.VideoAlbumId)
                .HasConstraintName("fk_video_album");
        });

        modelBuilder.Entity<WeeklyMenuEntries>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PK__weekly_m__810FDCE15E768653");

            entity.ToTable("weekly_menu_entries", tb => tb.HasTrigger("trg_wme_date_in_week_range"));

            entity.HasIndex(e => new { e.WeekId, e.BranchId, e.MenuDate }, "idx_wme_week_branch_date");

            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DishId).HasColumnName("dish_id");
            entity.Property(e => e.MealType)
                .HasMaxLength(20)
                .HasDefaultValue("ogle")
                .HasColumnName("meal_type");
            entity.Property(e => e.MenuDate).HasColumnName("menu_date");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.WeekId).HasColumnName("week_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.WeeklyMenuEntries)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wme_branch");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.WeeklyMenuEntries)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("fk_wme_creator");

            entity.HasOne(d => d.Dish).WithMany(p => p.WeeklyMenuEntries)
                .HasForeignKey(d => d.DishId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wme_dish");

            entity.HasOne(d => d.Week).WithMany(p => p.WeeklyMenuEntries)
                .HasForeignKey(d => d.WeekId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wme_week");
        });

        modelBuilder.Entity<Weeks>(entity =>
        {
            entity.HasKey(e => e.WeekId).HasName("PK__weeks__CEF979DFAA14767C");

            entity.ToTable("weeks");

            entity.HasIndex(e => new { e.Year, e.WeekNumber }, "uq_week").IsUnique();

            entity.Property(e => e.WeekId).HasColumnName("week_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.WeekNumber).HasColumnName("week_number");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
