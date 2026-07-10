using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Users
{
    public int UserId { get; set; }

    public string AzureObjectId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Title { get; set; }

    public int? DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? BirthMonth { get; set; }

    public int? BirthDay { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? Phone { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Announcements> Announcements { get; set; } = new List<Announcements>();

    public virtual Branches? Branch { get; set; }

    public virtual Departments? Department { get; set; }

    public virtual ICollection<EventParticipants> EventParticipants { get; set; } = new List<EventParticipants>();

    public virtual ICollection<Events> Events { get; set; } = new List<Events>();

    public virtual ICollection<PhotoAlbums> PhotoAlbums { get; set; } = new List<PhotoAlbums>();

    public virtual ICollection<Photos> Photos { get; set; } = new List<Photos>();

    public virtual ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();

    public virtual ICollection<VideoAlbums> VideoAlbums { get; set; } = new List<VideoAlbums>();

    public virtual ICollection<Videos> Videos { get; set; } = new List<Videos>();

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
