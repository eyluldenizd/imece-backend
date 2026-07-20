using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class WeeklyMenuItemService
{
    private readonly WeeklyMenuRepository _weeklyMenuRepository;
    private readonly WeeklyMenuItemRepository _weeklyMenuItemRepository;
    private readonly DishCategoryRepository _dishCategoryRepository;
    private readonly DishesRepository _dishesRepository;
    private readonly ICompanyContext _companyContext;

    public WeeklyMenuItemService(
        WeeklyMenuRepository weeklyMenuRepository,
        WeeklyMenuItemRepository weeklyMenuItemRepository,
        DishCategoryRepository dishCategoryRepository,
        DishesRepository dishesRepository,
        ICompanyContext companyContext)
    {
        _weeklyMenuRepository = weeklyMenuRepository;
        _weeklyMenuItemRepository = weeklyMenuItemRepository;
        _dishCategoryRepository = dishCategoryRepository;
        _dishesRepository = dishesRepository;
        _companyContext = companyContext;
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateWeeklyMenuItemDto request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult<long>.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var validationError = await ValidateItemAsync(
            menu,
            request.MenuDate,
            request.DishCategoryId,
            request.DishId,
            null,
            cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        var entity = new WeeklyMenuItems
        {
            MenuId = request.MenuId,
            MenuDate = request.MenuDate,
            DishCategoryId = request.DishCategoryId,
            DishId = request.DishId,
            SortOrder = request.SortOrder,
            Notes = request.Notes?.Trim()
        };

        try
        {
            var id = await _weeklyMenuItemRepository.CreateAsync(entity, cancellationToken);
            return ServiceResult<long>.Created(id);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return ServiceResult<long>.Conflict("Bu menü günü için aynı kategori ve yemek zaten eklenmiş.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateWeeklyMenuItemDto request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var existing = await _weeklyMenuItemRepository.GetByIdAndMenuIdAsync(
            request.MenuId,
            request.MenuItemId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult.NotFound("Menü öğesi bulunamadı.");
        }

        var validationError = await ValidateItemAsync(
            menu,
            request.MenuDate,
            request.DishCategoryId,
            request.DishId,
            request.MenuItemId,
            cancellationToken);

        if (validationError is not null)
        {
            return ConvertValidationError(validationError);
        }

        existing.MenuDate = request.MenuDate;
        existing.DishCategoryId = request.DishCategoryId;
        existing.DishId = request.DishId;
        existing.SortOrder = request.SortOrder;
        existing.Notes = request.Notes?.Trim();

        try
        {
            var rows = await _weeklyMenuItemRepository.UpdateAsync(existing, cancellationToken);
            return rows == 0
                ? ServiceResult.NotFound("Menü öğesi bulunamadı.")
                : ServiceResult.NoContent();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return ServiceResult.Conflict("Bu menü günü için aynı kategori ve yemek zaten eklenmiş.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(
        WeeklyMenuItemRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        var menu = await _weeklyMenuRepository.GetByIdAsync(request.MenuId, cancellationToken);
        if (menu is null)
        {
            return ServiceResult.NotFound("Haftalık menü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, menu.CompanyId);

        var rows = await _weeklyMenuItemRepository.SoftDeleteAsync(
            request.MenuId,
            request.MenuItemId,
            cancellationToken);

        return rows == 0
            ? ServiceResult.NotFound("Menü öğesi bulunamadı.")
            : ServiceResult.NoContent();
    }

    private async Task<ServiceResult<long>?> ValidateItemAsync(
        WeeklyMenus menu,
        DateOnly menuDate,
        int dishCategoryId,
        int dishId,
        long? excludeMenuItemId,
        CancellationToken cancellationToken)
    {
        if (menuDate < menu.PeriodStartDate || menuDate > menu.PeriodEndDate)
        {
            return ServiceResult<long>.BadRequest("Menü tarihi seçilen dönem aralığında olmalıdır.");
        }

        var category = await _dishCategoryRepository.GetByIdAsync(dishCategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            return ServiceResult<long>.BadRequest("Geçersiz yemek kategorisi.");
        }

        var dish = await _dishesRepository.GetByIdAsync(dishId, cancellationToken);
        if (dish is null || !dish.IsActive)
        {
            return ServiceResult<long>.BadRequest("Geçersiz yemek.");
        }

        if (dish.DishCategoryId != dishCategoryId)
        {
            return ServiceResult<long>.BadRequest("Seçilen yemek belirtilen kategoriye ait değil.");
        }

        if (await _weeklyMenuItemRepository.ExistsDuplicateAsync(
                menu.MenuId,
                menuDate,
                dishCategoryId,
                dishId,
                excludeMenuItemId,
                cancellationToken))
        {
            return ServiceResult<long>.Conflict("Bu menü günü için aynı kategori ve yemek zaten eklenmiş.");
        }

        return null;
    }

    private static ServiceResult ConvertValidationError(ServiceResult<long> validationError) =>
        validationError.StatusCode switch
        {
            StatusCodeEnum.BadRequest => ServiceResult.BadRequest(validationError.Message),
            StatusCodeEnum.Conflict => ServiceResult.Conflict(validationError.Message),
            StatusCodeEnum.NotFound => ServiceResult.NotFound(validationError.Message),
            _ => ServiceResult.BadRequest(validationError.Message)
        };
}
