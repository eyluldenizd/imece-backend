using Application.DTOs;

namespace Application.Common.ListQuery;

public static class AdminListQueryProfiles
{
    public static IReadOnlyList<UserDto> ApplyToUsers(
        IEnumerable<UserDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            user => user.FullName,
            user => user.Email,
            user => user.Title,
            user => user.CompanyName,
            user => user.BranchName,
            user => user.DepartmentName,
            user => user.RoleName,
            user => user.Phone,
            user => user.Username);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, user => user.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, user => user.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, user => user.DepartmentId);
        result = ContentListQueryApplier.ApplyRoleName(result, query, user => user.RoleName);
        result = ContentListQueryApplier.ApplyIsActive(result, query, user => user.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<UserDto, IComparable>>
            {
                ["user"] = user => user.FullName,
                ["email"] = user => user.Email,
                ["name"] = user => user.FullName,
            },
            items => items.OrderBy(user => user.FullName));
    }

    public static IReadOnlyList<BranchDto> ApplyToBranches(
        IEnumerable<BranchDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            branch => branch.BranchName,
            branch => branch.BranchCode,
            branch => branch.CompanyName,
            branch => branch.Description,
            branch => branch.BranchId.ToString());
        result = ContentListQueryApplier.ApplyCompanyId(result, query, branch => branch.CompanyId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, branch => branch.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<BranchDto, IComparable>>
            {
                ["name"] = branch => branch.BranchName,
                ["code"] = branch => branch.BranchCode,
            },
            items => items.OrderBy(branch => branch.BranchName));
    }

    public static IReadOnlyList<DepartmentDto> ApplyToDepartments(
        IEnumerable<DepartmentDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            department => department.DepartmentName,
            department => department.DepartmentCode,
            department => department.BranchName,
            department => department.CompanyName,
            department => department.Description,
            department => department.DepartmentId.ToString());
        result = ContentListQueryApplier.ApplyCompanyId(result, query, department => department.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, department => department.BranchId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, department => department.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<DepartmentDto, IComparable>>
            {
                ["name"] = department => department.DepartmentName,
                ["code"] = department => department.DepartmentCode,
            },
            items => items.OrderBy(department => department.DepartmentName));
    }

    public static IReadOnlyList<CompanyDto> ApplyToCompanies(
        IEnumerable<CompanyDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            company => company.CompanyName,
            company => company.CompanyCode,
            company => company.CompanyId.ToString());
        result = ContentListQueryApplier.ApplyIsActive(result, query, company => company.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CompanyDto, IComparable>>
            {
                ["name"] = company => company.CompanyName,
                ["code"] = company => company.CompanyCode,
            },
            items => items.OrderBy(company => company.CompanyName));
    }

    public static IReadOnlyList<AnnouncementDto> ApplyToAnnouncements(
        IEnumerable<AnnouncementDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            announcement => announcement.Title,
            announcement => announcement.Content);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, announcement => announcement.CompanyId);
        result = ContentListQueryApplier.ApplyIsPinned(result, query, announcement => announcement.IsPinned);
        result = ContentListQueryApplier.ApplyScopeType(result, query, announcement => announcement.ScopeType);
        result = ContentListQueryApplier.ApplyPublishWindowActive(result, query);
        result = ContentListQueryApplier.ApplyDateRange(result, query, announcement => announcement.PublishStart);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<AnnouncementDto, IComparable>>
            {
                ["title"] = announcement => announcement.Title,
                ["publishStart"] = announcement => announcement.PublishStart,
                ["publishEnd"] = announcement => announcement.PublishEnd ?? DateTime.MinValue,
                ["viewCount"] = announcement => announcement.ViewCount,
            },
            items => items.OrderByDescending(announcement => announcement.PublishStart));
    }

    public static IReadOnlyList<CampaignDto> ApplyToCampaigns(
        IEnumerable<CampaignDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            campaign => campaign.Title,
            campaign => campaign.Description,
            campaign => campaign.TargetUrl,
            campaign => campaign.CompanyName,
            campaign => campaign.BranchName,
            campaign => campaign.DepartmentName);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, campaign => campaign.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, campaign => campaign.BranchId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, campaign => campaign.IsActive);
        result = ContentListQueryApplier.ApplyDateRange(result, query, campaign => campaign.StartDate);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CampaignDto, IComparable>>
            {
                ["title"] = campaign => campaign.Title,
                ["startDate"] = campaign => campaign.StartDate,
                ["endDate"] = campaign => campaign.EndDate,
            },
            items => items.OrderByDescending(campaign => campaign.StartDate));
    }

    public static IReadOnlyList<EventDto> ApplyToEvents(
        IEnumerable<EventDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.Description,
            item => item.Location,
            item => item.EventType);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyScopeType(result, query, item => item.ScopeType);
        result = ContentListQueryApplier.ApplyType(result, query, item => item.EventType);
        result = ContentListQueryApplier.ApplyEventLifecycleStatus(
            result,
            query,
            item => item.StartDateTime,
            item => item.EndDateTime);
        result = ContentListQueryApplier.ApplyDateRange(result, query, item => item.StartDateTime);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<EventDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["start"] = item => item.StartDateTime,
                ["startDateTime"] = item => item.StartDateTime,
                ["end"] = item => item.EndDateTime,
                ["endDateTime"] = item => item.EndDateTime,
            },
            items => items.OrderByDescending(item => item.StartDateTime));
    }

    public static IReadOnlyList<ReservationDto> ApplyToReservations(
        IEnumerable<ReservationDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.RoomName,
            item => item.RequesterName,
            item => item.Description);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyMeetingRoomId(result, query, item => item.MeetingRoomId);
        result = ContentListQueryApplier.ApplyStatus(result, query, item => item.Status);
        result = ContentListQueryApplier.ApplyReservationRecordActive(result, query, item => item.Status);
        result = ContentListQueryApplier.ApplyStringContains(result, query, query?.RoomName, item => item.RoomName);
        result = ContentListQueryApplier.ApplyDateRange(result, query, item => item.StartTime);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<ReservationDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["startTime"] = item => item.StartTime,
                ["start"] = item => item.StartTime,
                ["endTime"] = item => item.EndTime,
                ["end"] = item => item.EndTime,
            },
            items => items.OrderByDescending(item => item.StartTime));
    }

    public static IReadOnlyList<SocialActivityDto> ApplyToSocialActivities(
        IEnumerable<SocialActivityDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.Description,
            item => item.Location,
            item => item.ActivityType,
            item => item.CompanyName,
            item => item.BranchName,
            item => item.DepartmentName);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyType(result, query, item => item.ActivityType);
        result = ContentListQueryApplier.ApplyStatus(result, query, item => item.Status);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);
        result = ContentListQueryApplier.ApplyDateRange(result, query, item => item.StartAt);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<SocialActivityDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["startAt"] = item => item.StartAt,
                ["start"] = item => item.StartAt,
            },
            items => items.OrderByDescending(item => item.StartAt));
    }

    public static IReadOnlyList<WeeklyMenuDto> ApplyToWeeklyMenus(
        IEnumerable<WeeklyMenuDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.MenuCode);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyIsPublished(result, query, item => item.IsPublished);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);
        result = ContentListQueryApplier.ApplyYear(result, query, item => item.Year);
        result = ContentListQueryApplier.ApplyMonth(result, query, item => item.Month);
        result = ContentListQueryApplier.ApplyDateRange(
            result,
            query,
            item => item.PeriodStartDate.ToDateTime(TimeOnly.MinValue));

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<WeeklyMenuDto, IComparable>>
            {
                ["title"] = item => item.Title ?? item.MenuCode,
                ["periodStart"] = item => item.PeriodStartDate,
                ["start"] = item => item.PeriodStartDate,
            },
            items => items.OrderByDescending(item => item.PeriodStartDate));
    }

    public static IReadOnlyList<MediaFileDto> ApplyToMediaFiles(
        IEnumerable<MediaFileDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.Description,
            item => item.OriginalFileName,
            item => item.DocumentNumber);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyType(result, query, item => item.MediaType);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);
        result = ContentListQueryApplier.ApplyDateRange(result, query, item => item.UploadedAt);
        result = ApplyMediaFeatureScope(result, query);
        result = ContentListQueryApplier.ApplyMediaDocumentType(result, query);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<MediaFileDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["uploadedAt"] = item => item.UploadedAt,
                ["createdAt"] = item => item.UploadedAt,
                ["date"] = item => item.UploadedAt,
            },
            items => items.OrderByDescending(item => item.UploadedAt));
    }

    public static IReadOnlyList<MeetingRoomDto> ApplyToMeetingRooms(
        IEnumerable<MeetingRoomDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Code,
            item => item.Floor,
            item => item.Features,
            item => item.LocationDescription);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyStringContains(result, query, query?.Floor, item => item.Floor);
        result = ContentListQueryApplier.ApplyStringContains(result, query, query?.Feature, item => item.Features);
        result = ContentListQueryApplier.ApplyExactString(result, query, query?.RoomName, item => item.Name);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<MeetingRoomDto, IComparable>>
            {
                ["name"] = item => item.Name,
                ["capacity"] = item => item.Capacity,
            },
            items => items.OrderBy(item => item.Name));
    }

    public static IReadOnlyList<CorporateAppDto> ApplyToCorporateApps(
        IEnumerable<CorporateAppDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.Description,
            item => item.Url,
            item => item.CategoryName,
            item => item.Category);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyCategoryId(result, query, item => item.CorporateAppCategoryId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CorporateAppDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["name"] = item => item.Title,
            },
            items => items.OrderBy(item => item.Title));
    }

    public static IReadOnlyList<CommunicationChannelDto> ApplyToCommunicationChannels(
        IEnumerable<CommunicationChannelDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.ChannelName,
            item => item.Type,
            item => item.AddressUrl,
            item => item.DepartmentInCharge,
            item => item.Description,
            item => item.CompanyName,
            item => item.BranchName,
            item => item.DepartmentName);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyCategoryId(result, query, item => item.CommunicationChannelTypeId);
        result = ContentListQueryApplier.ApplyType(result, query, item => item.Type);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CommunicationChannelDto, IComparable>>
            {
                ["name"] = item => item.ChannelName,
                ["channelName"] = item => item.ChannelName,
                ["sortOrder"] = item => item.SortOrder,
            },
            items => items.OrderBy(item => item.SortOrder).ThenBy(item => item.ChannelName));
    }

    public static IReadOnlyList<ServiceLocationDto> ApplyToServiceLocations(
        IEnumerable<ServiceLocationDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.LocationType,
            item => item.TypeName,
            item => item.Address);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyCategoryId(result, query, item => item.ServiceLocationTypeId);
        result = ContentListQueryApplier.ApplyType(result, query, item => item.LocationType);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<ServiceLocationDto, IComparable>>
            {
                ["name"] = item => item.Name,
            },
            items => items.OrderBy(item => item.Name));
    }

    public static IReadOnlyList<ServiceRouteDto> ApplyToServiceRoutes(
        IEnumerable<ServiceRouteDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.RouteName,
            item => item.DepartureLocation,
            item => item.ArrivalLocation,
            item => item.RouteDescription);
        result = ContentListQueryApplier.ApplyStringContains(
            result,
            query,
            query?.Departure,
            item => item.DepartureLocation);
        result = ContentListQueryApplier.ApplyStringContains(
            result,
            query,
            query?.Arrival,
            item => item.ArrivalLocation);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<ServiceRouteDto, IComparable>>
            {
                ["name"] = item => item.RouteName,
                ["routeName"] = item => item.RouteName,
                ["displayOrder"] = item => item.DisplayOrder ?? int.MaxValue,
            },
            items => items.OrderBy(item => item.DisplayOrder ?? int.MaxValue).ThenBy(item => item.RouteName));
    }

    public static IReadOnlyList<ServiceDto> ApplyToServices(
        IEnumerable<ServiceDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Description,
            item => item.CompanyName,
            item => item.BranchName,
            item => item.DepartmentName);
        result = ContentListQueryApplier.ApplyCompanyId(result, query, item => item.CompanyId);
        result = ContentListQueryApplier.ApplyBranchId(result, query, item => item.BranchId);
        result = ContentListQueryApplier.ApplyDepartmentId(result, query, item => item.DepartmentId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<ServiceDto, IComparable>>
            {
                ["name"] = item => item.Name,
            },
            items => items.OrderBy(item => item.Name));
    }

    public static IReadOnlyList<DishesDto> ApplyToDishes(
        IEnumerable<DishesDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.DishName,
            item => item.Description,
            item => item.DishCategoryName,
            item => item.Category);
        result = ContentListQueryApplier.ApplyCategoryId(result, query, item => item.DishCategoryId);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<DishesDto, IComparable>>
            {
                ["name"] = item => item.DishName,
                ["dishName"] = item => item.DishName,
                ["createdAt"] = item => item.CreatedAt,
            },
            items => items.OrderBy(item => item.DishName));
    }

    public static IReadOnlyList<DishCategoryDto> ApplyToDishCategories(
        IEnumerable<DishCategoryDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Code,
            item => item.Description);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<DishCategoryDto, IComparable>>
            {
                ["name"] = item => item.Name,
                ["sortOrder"] = item => item.SortOrder,
            },
            items => items.OrderBy(item => item.SortOrder).ThenBy(item => item.Name));
    }

    public static IReadOnlyList<CorporateAppCategoryDto> ApplyToCorporateAppCategories(
        IEnumerable<CorporateAppCategoryDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Description);
        result = ContentListQueryApplier.ApplyStringContains(result, query, query?.ColorKey, item => item.ColorKey);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CorporateAppCategoryDto, IComparable>>
            {
                ["name"] = item => item.Name,
                ["sortOrder"] = item => item.SortOrder,
            },
            items => items.OrderBy(item => item.SortOrder).ThenBy(item => item.Name));
    }

    public static IReadOnlyList<ServiceLocationTypeDto> ApplyToServiceLocationTypes(
        IEnumerable<ServiceLocationTypeDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Description);
        result = ContentListQueryApplier.ApplyStringContains(result, query, query?.ColorKey, item => item.ColorKey);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<ServiceLocationTypeDto, IComparable>>
            {
                ["name"] = item => item.Name,
                ["sortOrder"] = item => item.SortOrder,
            },
            items => items.OrderBy(item => item.SortOrder).ThenBy(item => item.Name));
    }

    public static IReadOnlyList<CommunicationChannelTypeDto> ApplyToCommunicationChannelTypes(
        IEnumerable<CommunicationChannelTypeDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Name,
            item => item.Code);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<CommunicationChannelTypeDto, IComparable>>
            {
                ["name"] = item => item.Name,
                ["sortOrder"] = item => item.SortOrder,
            },
            items => items.OrderBy(item => item.SortOrder).ThenBy(item => item.Name));
    }

    public static IReadOnlyList<RoleListItemDto> ApplyToRoles(
        IEnumerable<RoleListItemDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.RoleName,
            item => item.Description);
        result = ContentListQueryApplier.ApplyRoleName(result, query, item => item.RoleName);
        result = ContentListQueryApplier.ApplyIsActive(result, query, item => item.IsActive);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<RoleListItemDto, IComparable>>
            {
                ["name"] = item => item.RoleName,
                ["roleName"] = item => item.RoleName,
            },
            items => items.OrderBy(item => item.RoleName));
    }

    public static IReadOnlyList<UpcomingEventDto> ApplyToUpcomingEvents(
        IEnumerable<UpcomingEventDto> source,
        ContentListQueryDto? query)
    {
        var result = source;
        result = ContentListQueryApplier.ApplyMultiFieldSearch(
            result,
            query,
            item => item.Title,
            item => item.Description,
            item => item.Location);
        result = ContentListQueryApplier.ApplyDateRange(result, query, item => item.EventDate);

        return ContentListQueryApplier.ApplySort(
            result,
            query,
            new Dictionary<string, Func<UpcomingEventDto, IComparable>>
            {
                ["title"] = item => item.Title,
                ["eventDate"] = item => item.EventDate,
                ["date"] = item => item.EventDate,
            },
            items => items.OrderBy(item => item.EventDate));
    }

    private static IEnumerable<MediaFileDto> ApplyMediaFeatureScope(
        IEnumerable<MediaFileDto> source,
        ContentListQueryDto? query)
    {
        if (string.IsNullOrWhiteSpace(query?.FeatureType))
        {
            return source;
        }

        var featureType = query.FeatureType.Trim();
        return source.Where(file =>
        {
            if (featureType.Equals("Document", StringComparison.OrdinalIgnoreCase))
            {
                return file.MediaType.Equals("Document", StringComparison.OrdinalIgnoreCase);
            }

            if (featureType.Equals("Gallery", StringComparison.OrdinalIgnoreCase))
            {
                return file.MediaType.Equals("Photo", StringComparison.OrdinalIgnoreCase)
                    && file.RelativePath.Contains("gallery", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        });
    }
}
