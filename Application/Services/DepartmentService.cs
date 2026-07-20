using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DepartmentService
{
    private readonly DepartmentRepository _departmentRepository;
    private readonly BranchRepository _branchRepository;
    private readonly ICompanyContext _companyContext;

    public DepartmentService(
        DepartmentRepository departmentRepository,
        BranchRepository branchRepository,
        ICompanyContext companyContext)
    {
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _companyContext = companyContext;
    }

    public async Task<ServiceResult<IReadOnlyList<DepartmentDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<DepartmentDto>>.Success(
            FilterAccessible(departments).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<DepartmentDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetActiveAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<DepartmentDto>>.Success(
            FilterAccessible(departments).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<DepartmentDto>>> GetByBranchIdAsync(
        BranchIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch?.CompanyId is int companyId)
        {
            _companyContext.EnsureCanAccessCompany(companyId);
        }
        else if (!_companyContext.IsGlobalAdmin)
        {
            throw new Application.Exceptions.ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }

        var departments = await _departmentRepository.GetByBranchIdAsync(
            request.BranchId,
            cancellationToken);

        return ServiceResult<IReadOnlyList<DepartmentDto>>.Success(
            departments.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<DepartmentDto>>> GetByCompanyIdAsync(
        CompanyIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var departments = await _departmentRepository.GetByCompanyIdAsync(
            request.CompanyId,
            cancellationToken);

        return ServiceResult<IReadOnlyList<DepartmentDto>>.Success(
            departments.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<DepartmentDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (department is null)
        {
            return ServiceResult<DepartmentDto>.NotFound(
                $"ID değeri {request.Id} olan departman bulunamadı.");
        }

        EnsureDepartmentAccess(department);

        return ServiceResult<DepartmentDto>.Success(ToDto(department));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateDepartmentDto request,
        CancellationToken cancellationToken = default)
    {
        var branchValidation = await ValidateBranchForWriteAsync(request.BranchId, null, cancellationToken);
        if (branchValidation is not null)
        {
            return ServiceResult<int>.BadRequest(branchValidation.Message!);
        }

        var departmentCode = request.DepartmentCode?.Trim();
        if (!string.IsNullOrWhiteSpace(departmentCode)
            && await _departmentRepository.ExistsByCodeInBranchAsync(
                request.BranchId,
                departmentCode,
                cancellationToken: cancellationToken))
        {
            return ServiceResult<int>.Conflict(
                "Bu şube için departman kodu zaten kullanılıyor.");
        }

        var entity = new DepartmentRecord
        {
            BranchId = request.BranchId,
            ParentDepartmentId = request.ParentDepartmentId,
            DepartmentCode = departmentCode,
            DepartmentName = request.DepartmentName.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        var departmentId = await _departmentRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(departmentId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateDepartmentDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _departmentRepository.GetByIdAsync(
            request.DepartmentId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.DepartmentId} olan departman bulunamadı.");
        }

        EnsureDepartmentAccess(existing);

        var branchValidation = await ValidateBranchForWriteAsync(
            request.BranchId,
            existing.CompanyId,
            cancellationToken);
        if (branchValidation is not null)
        {
            return branchValidation;
        }

        var departmentCode = request.DepartmentCode?.Trim();
        if (!string.IsNullOrWhiteSpace(departmentCode)
            && await _departmentRepository.ExistsByCodeInBranchAsync(
                request.BranchId,
                departmentCode,
                request.DepartmentId,
                cancellationToken))
        {
            return ServiceResult.Conflict(
                "Bu şube için departman kodu zaten kullanılıyor.");
        }

        existing.BranchId = request.BranchId;
        existing.ParentDepartmentId = request.ParentDepartmentId;
        existing.DepartmentCode = departmentCode;
        existing.DepartmentName = request.DepartmentName.Trim();
        existing.Description = request.Description?.Trim();
        existing.IsActive = request.IsActive;

        var rows = await _departmentRepository.UpdateAsync(existing, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.Conflict("Departman güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _departmentRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan departman bulunamadı veya zaten pasif.");
        }

        EnsureDepartmentAccess(existing);

        var rows = await _departmentRepository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan departman bulunamadı veya zaten pasif.");
        }

        return ServiceResult.NoContent();
    }

    private async Task<ServiceResult?> ValidateBranchForWriteAsync(
        int branchId,
        int? expectedCompanyId,
        CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch is null)
        {
            return ServiceResult.BadRequest("Geçersiz şube ID değeri.");
        }

        if (!branch.CompanyId.HasValue)
        {
            return ServiceResult.BadRequest("Şube bir şirkete bağlı değil.");
        }

        if (expectedCompanyId.HasValue && branch.CompanyId.Value != expectedCompanyId.Value)
        {
            return ServiceResult.BadRequest(
                "Departmanın şubesi belirtilen şirkete ait değil.");
        }

        _companyContext.EnsureCanAccessCompany(branch.CompanyId.Value);
        return null;
    }

    private IEnumerable<DepartmentRecord> FilterAccessible(IEnumerable<DepartmentRecord> departments)
    {
        if (_companyContext.IsGlobalAdmin)
        {
            return departments;
        }

        return departments.Where(department =>
            department.CompanyId.HasValue
            && _companyContext.CanAccessCompany(department.CompanyId.Value));
    }

    private void EnsureDepartmentAccess(DepartmentRecord department)
    {
        if (department.CompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(department.CompanyId.Value);
        }
        else if (!_companyContext.IsGlobalAdmin)
        {
            throw new Application.Exceptions.ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }
    }

    private static DepartmentDto ToDto(DepartmentRecord entity) => new()
    {
        DepartmentId = entity.DepartmentId,
        BranchId = entity.BranchId,
        CompanyId = entity.CompanyId,
        ParentDepartmentId = entity.ParentDepartmentId,
        DepartmentCode = entity.DepartmentCode,
        DepartmentName = entity.DepartmentName,
        Description = entity.Description,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
