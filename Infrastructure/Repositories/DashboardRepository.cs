using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class DashboardRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public DashboardRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<int> CountUsersAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountUsers, filter, cancellationToken);

    public Task<int> CountActiveCompaniesAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountActiveCompanies, filter, cancellationToken);

    public Task<int> CountBranchesAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountBranches, filter, cancellationToken);

    public Task<int> CountDepartmentsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountDepartments, filter, cancellationToken);

    public Task<int> CountAnnouncementsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountAnnouncements, filter, cancellationToken);

    public Task<int> CountUpcomingEventsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountUpcomingEvents, filter, cancellationToken);

    public Task<int> CountActiveServicesAsync(CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(DashboardQueries.CountActiveServices, null, cancellationToken);

    public Task<int> CountMediaFilesAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountMediaFiles, filter, cancellationToken);

    public Task<int> CountReservationsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountReservations, filter, cancellationToken);

    public Task<int> CountPublishedWeeklyMenusAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountPublishedWeeklyMenus, filter, cancellationToken);

    public Task<int> CountGalleryAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountGallery, filter, cancellationToken);

    public Task<int> CountLibraryAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountLibrary, filter, cancellationToken);

    public Task<int> CountSocialActivitiesAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountSocialActivities, filter, cancellationToken);

    public Task<int> CountHistoryAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountHistory, filter, cancellationToken);

    public Task<int> CountShuttleRoutesAsync(CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(DashboardQueries.CountShuttleRoutes, null, cancellationToken);

    public Task<int> CountCampaignsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountCampaigns, filter, cancellationToken);

    public Task<int> CountEmergencyNumbersAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountEmergencyNumbers, filter, cancellationToken);

    public Task<int> CountECardsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountECards, filter, cancellationToken);

    public Task<int> CountMeetingRoomsAsync(CompanyListFilter filter, CancellationToken cancellationToken = default)
        => CountAsync(DashboardQueries.CountMeetingRooms, filter, cancellationToken);

    public Task<List<DashboardEventsByMonthRow>> GetEventsByMonthAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<DashboardEventsByMonthRow>(
            DashboardQueries.EventsByMonth,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    public Task<List<DashboardUsersByCompanyRow>> GetUsersByCompanyAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<DashboardUsersByCompanyRow>(
            DashboardQueries.UsersByCompany,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    private Task<int> CountAsync(string query, CompanyListFilter filter, CancellationToken cancellationToken)
        => _dataAccess.ExecuteScalarAsync<int>(
            query,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);
}

public sealed class DashboardEventsByMonthRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public sealed class DashboardUsersByCompanyRow
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int Count { get; set; }
}
