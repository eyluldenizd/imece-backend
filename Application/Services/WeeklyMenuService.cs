using Application.Common.CompanyScope;
using Application.Common.MealMenu;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class WeeklyMenuService
{
    private readonly WeeklyMenuRepository _weeklyMenuRepository;
    private readonly WeeklyMenuItemRepository _weeklyMenuItemRepository;
    private readonly DishCategoryRepository _dishCategoryRepository;
    private readonly DishesRepository _dishesRepository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public WeeklyMenuService(
        WeeklyMenuRepository weeklyMenuRepository,
        WeeklyMenuItemRepository weeklyMenuItemRepository,
        DishCategoryRepository dishCategoryRepository,
        DishesRepository dishesRepository,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _weeklyMenuRepository = weeklyMenuRepository;
        _weeklyMenuItemRepository = weeklyMenuItemRepository;
        _dishCategoryRepository = dishCategoryRepository;
        _dishesRepository = dishesRepository;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var menus = await _weeklyMenuRepository.GetAllAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        return ServiceResult<IReadOnlyList<WeeklyMenuDto>>.Success(
            menus.Select(menu => ToDto(menu, [])).ToList());
    }

    public async Task<ServiceResult<WeeklyMenuDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.Id, cancellationToken);
        if (menu is null)
        {
            return ServiceResult<WeeklyMenuDto>.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var items = await _weeklyMenuItemRepository.GetByMenuIdAsync(menu.MenuId, cancellationToken);
        var itemDtos = await MapItemsAsync(items, cancellationToken);

        return ServiceResult<WeeklyMenuDto>.Success(ToDto(menu, itemDtos));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateWeeklyMenuDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        if (!MenuCodeHelper.TryGetPeriodDates(
                request.Year,
                request.Month,
                request.WeekOfMonth,
                out var periodStart,
                out var periodEnd))
        {
            return ServiceResult<long>.BadRequest(
                "Seçilen ay ve hafta için geçerli bir dönem bulunamadı.");
        }

        var menuCode = MenuCodeHelper.GenerateMenuCode(
            request.Year,
            request.Month,
            request.WeekOfMonth);

        var existing = await _weeklyMenuRepository.GetByCompanyAndCodeAsync(
            request.CompanyId,
            menuCode,
            cancellationToken);

        if (existing is not null)
        {
            return ServiceResult<long>.Conflict(
                "Bu şirket için seçilen döneme ait menü zaten mevcut.");
        }

        var entity = new WeeklyMenus
        {
            CompanyId = request.CompanyId,
            MenuCode = menuCode,
            Year = request.Year,
            Month = request.Month,
            WeekOfMonth = request.WeekOfMonth,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            Title = request.Title?.Trim(),
            CreatedBy = _currentUser.GetRequiredUserId()
        };

        var menuId = await _weeklyMenuRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(menuId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateWeeklyMenuDto request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        menu.Title = request.Title?.Trim();
        await _weeklyMenuRepository.UpdateAsync(menu, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> PublishAsync(
        WeeklyMenuRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var rows = await _weeklyMenuRepository.PublishAsync(request.MenuId, cancellationToken);
        return rows == 0
            ? ServiceResult.NotFound("Haftalık menü bulunamadı.")
            : ServiceResult.NoContent();
    }

    public async Task<ServiceResult> UnpublishAsync(
        WeeklyMenuRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var rows = await _weeklyMenuRepository.UnpublishAsync(request.MenuId, cancellationToken);
        return rows == 0
            ? ServiceResult.NotFound("Haftalık menü bulunamadı.")
            : ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.Id, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var rows = await _weeklyMenuRepository.SoftDeleteAsync(request.Id, cancellationToken);
        return rows == 0
            ? ServiceResult.NotFound("Haftalık menü bulunamadı.")
            : ServiceResult.NoContent();
    }

    private async Task<IReadOnlyList<WeeklyMenuItemDto>> MapItemsAsync(
        IReadOnlyList<WeeklyMenuItems> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return [];
        }

        var categories = await _dishCategoryRepository.GetAllAsync(cancellationToken);
        var categoryMap = categories.ToDictionary(category => category.DishCategoryId);

        var dishIds = items.Select(item => item.DishId).Distinct().ToArray();
        var dishMap = new Dictionary<int, Dishes>();

        foreach (var dishId in dishIds)
        {
            var dish = await _dishesRepository.GetByIdAsync(dishId, cancellationToken);
            if (dish is not null)
            {
                dishMap[dishId] = dish;
            }
        }

        return items
            .Select(item =>
            {
                categoryMap.TryGetValue(item.DishCategoryId, out var category);
                dishMap.TryGetValue(item.DishId, out var dish);

                return new WeeklyMenuItemDto
                {
                    MenuItemId = item.MenuItemId,
                    MenuId = item.MenuId,
                    MenuDate = item.MenuDate,
                    DishCategoryId = item.DishCategoryId,
                    DishCategoryName = category?.Name,
                    DishId = item.DishId,
                    DishName = dish?.DishName,
                    SortOrder = item.SortOrder,
                    Notes = item.Notes,
                    IsActive = item.IsActive,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                };
            })
            .ToList();
    }

    private static WeeklyMenuDto ToDto(WeeklyMenus menu, IReadOnlyList<WeeklyMenuItemDto> items) => new()
    {
        MenuId = menu.MenuId,
        CompanyId = menu.CompanyId,
        MenuCode = menu.MenuCode,
        Year = menu.Year,
        Month = menu.Month,
        WeekOfMonth = menu.WeekOfMonth,
        PeriodStartDate = menu.PeriodStartDate,
        PeriodEndDate = menu.PeriodEndDate,
        Title = menu.Title,
        IsPublished = menu.IsPublished,
        PublishedAt = menu.PublishedAt,
        IsActive = menu.IsActive,
        CreatedBy = menu.CreatedBy,
        CreatedAt = menu.CreatedAt,
        UpdatedAt = menu.UpdatedAt,
        Items = items
    };
}
