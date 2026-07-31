using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class WeeklyMenuEntryService
{
    private readonly WeeklyMenuEntryRepository _weeklyMenuEntryRepository;
    private readonly BranchRepository _branchRepository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public WeeklyMenuEntryService(
        WeeklyMenuEntryRepository weeklyMenuEntryRepository,
        BranchRepository branchRepository,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _weeklyMenuEntryRepository = weeklyMenuEntryRepository;
        _branchRepository = branchRepository;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var entries = await _weeklyMenuEntryRepository.GetAllAsync(filter, cancellationToken);
        return ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>.Success(
            entries.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<WeeklyMenuEntryDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _weeklyMenuEntryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<WeeklyMenuEntryDto>.NotFound(
                $"ID değeri {request.Id} olan menü kaydı bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, entity.CompanyId);
        return ServiceResult<WeeklyMenuEntryDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>> GetCurrentWeekAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var entries = await _weeklyMenuEntryRepository.GetCurrentWeekAsync(filter, cancellationToken);
        return ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>.Success(
            entries.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>> GetByDateAsync(
        WeeklyMenuDateRequest request,
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var entries = await _weeklyMenuEntryRepository.GetByDateAsync(
            request.MenuDate,
            filter,
            cancellationToken);
        return ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>.Success(
            entries.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>> GetByBranchAsync(
        WeeklyMenuBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(request.BranchId, cancellationToken);
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var entries = await _weeklyMenuEntryRepository.GetByBranchAsync(
            request.BranchId,
            filter,
            cancellationToken);
        return ServiceResult<IReadOnlyList<WeeklyMenuEntryDto>>.Success(
            entries.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateWeeklyMenuEntryDto request,
        CancellationToken cancellationToken = default)
    {
        await EnsureBranchAccessAsync(request.BranchId, cancellationToken);

        var isMenuDateInWeek = await _weeklyMenuEntryRepository.IsMenuDateInWeekAsync(
            request.WeekId,
            request.MenuDate,
            cancellationToken);

        if (!isMenuDateInWeek)
        {
            return ServiceResult<long>.BadRequest(
                "Menü tarihi, seçilen haftanın başlangıç ve bitiş tarihleri arasında olmalıdır.");
        }

        if (request.SortOrder < 0)
        {
            return ServiceResult<long>.BadRequest("Sıralama değeri negatif olamaz.");
        }

        var entity = new WeeklyMenuEntries
        {
            WeekId = request.WeekId,
            DishId = request.DishId,
            BranchId = request.BranchId,
            MenuDate = request.MenuDate,
            MealType = request.MealType.Trim(),
            SortOrder = request.SortOrder,
            CreatedBy = request.CreatedBy
        };

        var entryId = await _weeklyMenuEntryRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(entryId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateWeeklyMenuEntryDto request,
        CancellationToken cancellationToken = default)
    {
        var existingEntity = await _weeklyMenuEntryRepository.GetByIdAsync(
            request.EntryId,
            cancellationToken);

        if (existingEntity is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.EntryId} olan menü kaydı bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, existingEntity.CompanyId);
        await EnsureBranchAccessAsync(request.BranchId, cancellationToken);

        var isMenuDateInWeek = await _weeklyMenuEntryRepository.IsMenuDateInWeekAsync(
            request.WeekId,
            request.MenuDate,
            cancellationToken);

        if (!isMenuDateInWeek)
        {
            return ServiceResult.BadRequest(
                "Menü tarihi, seçilen haftanın başlangıç ve bitiş tarihleri arasında olmalıdır.");
        }

        if (request.SortOrder < 0)
        {
            return ServiceResult.BadRequest("Sıralama değeri negatif olamaz.");
        }

        var entity = new WeeklyMenuEntries
        {
            EntryId = request.EntryId,
            WeekId = request.WeekId,
            DishId = request.DishId,
            BranchId = request.BranchId,
            MenuDate = request.MenuDate,
            MealType = request.MealType.Trim(),
            SortOrder = request.SortOrder,
            CreatedBy = request.CreatedBy
        };

        var rowsAffected = await _weeklyMenuEntryRepository.UpdateAsync(entity, cancellationToken);
        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict("Haftalık menü kaydı güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _weeklyMenuEntryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan menü kaydı bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, existing.CompanyId);
        await _weeklyMenuEntryRepository.DeleteAsync(request.Id, cancellationToken);
        return ServiceResult.NoContent();
    }

    private async Task EnsureBranchAccessAsync(int branchId, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch is null)
        {
            throw new Application.Exceptions.ForbiddenException("Geçersiz şube.");
        }

        if (branch.CompanyId.HasValue)
        {
            CompanyScopeRules.EnsureCompanyAccess(_companyContext, branch.CompanyId.Value);
        }
    }

    private static WeeklyMenuEntryDto ToDto(WeeklyMenuEntryDetails entity) =>
        new()
        {
            EntryId = entity.EntryId,
            WeekId = entity.WeekId,
            Year = entity.Year,
            WeekNumber = entity.WeekNumber,
            WeekStartDate = entity.WeekStartDate,
            WeekEndDate = entity.WeekEndDate,
            DishId = entity.DishId,
            DishName = entity.DishName,
            DishCategory = entity.DishCategory,
            BranchId = entity.BranchId,
            BranchName = entity.BranchName,
            MenuDate = entity.MenuDate,
            MealType = entity.MealType,
            SortOrder = entity.SortOrder,
            CreatedBy = entity.CreatedBy,
            CreatedByFullName = entity.CreatedByFullName,
            CreatedAt = entity.CreatedAt
        };
}
