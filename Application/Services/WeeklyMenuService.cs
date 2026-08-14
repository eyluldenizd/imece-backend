using Application.Common.CompanyScope;
using Application.Common.ListQuery;
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
    private readonly BranchRepository _branchRepository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public WeeklyMenuService(
        WeeklyMenuRepository weeklyMenuRepository,
        WeeklyMenuItemRepository weeklyMenuItemRepository,
        DishCategoryRepository dishCategoryRepository,
        DishesRepository dishesRepository,
        BranchRepository branchRepository,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _weeklyMenuRepository = weeklyMenuRepository;
        _weeklyMenuItemRepository = weeklyMenuItemRepository;
        _dishCategoryRepository = dishCategoryRepository;
        _dishesRepository = dishesRepository;
        _branchRepository = branchRepository;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var menus = await _weeklyMenuRepository.GetAllAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        var dtos = menus.Select(menu => ToDto(menu, [])).ToList();
        return ServiceResult<IReadOnlyList<WeeklyMenuDto>>.Success(
            AdminListQueryProfiles.ApplyToWeeklyMenus(dtos, query));
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

        var branchValidation = await ValidateBranchForWriteAsync(
            request.BranchId,
            request.CompanyId,
            cancellationToken);
        if (branchValidation is not null)
        {
            return ServiceResult<long>.BadRequest(branchValidation);
        }

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

        var existing = await _weeklyMenuRepository.GetByCompanyBranchAndCodeAsync(
            request.CompanyId,
            request.BranchId,
            menuCode,
            cancellationToken);

        if (existing is not null)
        {
            return ServiceResult<long>.Conflict(
                "Bu şirket ve şube için seçilen döneme ait menü zaten mevcut.");
        }

        var entity = new WeeklyMenus
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
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
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var branchValidation = await ValidateBranchForWriteAsync(
            request.BranchId,
            request.CompanyId,
            cancellationToken);
        if (branchValidation is not null)
        {
            return ServiceResult.BadRequest(branchValidation);
        }

        var menuCode = menu.MenuCode;
        var conflicting = await _weeklyMenuRepository.GetByCompanyBranchAndCodeAsync(
            request.CompanyId,
            request.BranchId,
            menuCode,
            cancellationToken);
        if (conflicting is not null && conflicting.MenuId != menu.MenuId)
        {
            return ServiceResult.Conflict(
                "Bu şirket ve şube için seçilen döneme ait menü zaten mevcut.");
        }

        menu.Title = request.Title?.Trim();
        menu.CompanyId = request.CompanyId;
        menu.BranchId = request.BranchId;
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

    private async Task<string?> ValidateBranchForWriteAsync(
        int branchId,
        int expectedCompanyId,
        CancellationToken cancellationToken)
    {
        if (branchId <= 0)
        {
            return "Şube seçilmelidir.";
        }

        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch is null)
        {
            return "Geçersiz şube.";
        }

        if (!branch.CompanyId.HasValue)
        {
            return "Şube bir şirkete bağlı değil.";
        }

        if (branch.CompanyId.Value != expectedCompanyId)
        {
            return "Seçilen şube belirtilen şirkete ait değil.";
        }

        _companyContext.EnsureCanAccessCompany(branch.CompanyId.Value);
        return null;
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
        CompanyName = menu.CompanyName,
        BranchId = menu.BranchId,
        BranchName = menu.BranchName,
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
