using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DashboardService
{
    private readonly DashboardRepository _dashboardRepository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public DashboardService(
        DashboardRepository dashboardRepository,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _dashboardRepository = dashboardRepository;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);

        var summary = new DashboardSummaryDto
        {
            Users = await _dashboardRepository.CountUsersAsync(filter, cancellationToken),
            ActiveCompanies = await _dashboardRepository.CountActiveCompaniesAsync(filter, cancellationToken),
            Branches = await _dashboardRepository.CountBranchesAsync(filter, cancellationToken),
            Departments = await _dashboardRepository.CountDepartmentsAsync(filter, cancellationToken),
            Announcements = await _dashboardRepository.CountAnnouncementsAsync(filter, cancellationToken),
            UpcomingEvents = await _dashboardRepository.CountUpcomingEventsAsync(filter, cancellationToken),
            ActiveServices = await _dashboardRepository.CountActiveServicesAsync(cancellationToken),
            MediaFiles = await _dashboardRepository.CountMediaFilesAsync(filter, cancellationToken),
            Reservations = await _dashboardRepository.CountReservationsAsync(filter, cancellationToken),
            PublishedWeeklyMenus = await _dashboardRepository.CountPublishedWeeklyMenusAsync(filter, cancellationToken)
        };

        var eventsByMonth = await _dashboardRepository.GetEventsByMonthAsync(filter, cancellationToken);
        summary.EventsByMonth = eventsByMonth
            .Select(row => new DashboardEventsByMonthDto
            {
                Year = row.Year,
                Month = row.Month,
                Count = row.Count
            })
            .ToList();

        var usersByCompany = await _dashboardRepository.GetUsersByCompanyAsync(filter, cancellationToken);
        summary.UsersByCompany = usersByCompany
            .Select(row => new DashboardUsersByCompanyDto
            {
                CompanyId = row.CompanyId,
                CompanyName = row.CompanyName,
                Count = row.Count
            })
            .ToList();

        return ServiceResult<DashboardSummaryDto>.Success(summary);
    }
}
