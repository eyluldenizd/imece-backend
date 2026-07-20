using Application.Common.Codes;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class BranchService
{
    private readonly BranchRepository _branchRepository;
    private readonly ICompanyContext _companyContext;
    private readonly IEntityCodeGenerator _codeGenerator;

    public BranchService(
        BranchRepository branchRepository,
        ICompanyContext companyContext,
        IEntityCodeGenerator codeGenerator)
    {
        _branchRepository = branchRepository;
        _companyContext = companyContext;
        _codeGenerator = codeGenerator;
    }

    public async Task<ServiceResult<IReadOnlyList<BranchDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<BranchDto>>.Success(
            FilterAccessible(branches).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<BranchDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetActiveAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<BranchDto>>.Success(
            FilterAccessible(branches).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<BranchDto>>> GetByCompanyIdAsync(
        CompanyIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var branches = await _branchRepository.GetByCompanyIdAsync(
            request.CompanyId,
            cancellationToken);

        return ServiceResult<IReadOnlyList<BranchDto>>.Success(
            branches.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<BranchDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (branch is null)
        {
            return ServiceResult<BranchDto>.NotFound(
                $"ID değeri {request.Id} olan şube bulunamadı.");
        }

        EnsureBranchAccess(branch);

        return ServiceResult<BranchDto>.Success(ToDto(branch));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateBranchDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var branchCode = await _codeGenerator.AllocateAsync(
            request.BranchCode,
            request.BranchName,
            "BR",
            async (candidate, ct) =>
                await _branchRepository.ExistsByCodeInCompanyAsync(
                    request.CompanyId,
                    candidate,
                    cancellationToken: ct),
            maxLength: 64,
            cancellationToken);

        var entity = new Branches
        {
            CompanyId = request.CompanyId,
            BranchCode = branchCode,
            BranchName = request.BranchName.Trim(),
            Description = request.Description?.Trim(),
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = request.IsActive
        };

        var branchId = await _branchRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(branchId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateBranchDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var existing = await _branchRepository.GetByIdAsync(
            request.BranchId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.BranchId} olan şube bulunamadı.");
        }

        EnsureBranchAccess(existing);

        var branchCode = string.IsNullOrWhiteSpace(request.BranchCode)
            ? existing.BranchCode
            : await _codeGenerator.AllocateAsync(
                request.BranchCode,
                request.BranchName,
                "BR",
                async (candidate, ct) =>
                    await _branchRepository.ExistsByCodeInCompanyAsync(
                        request.CompanyId,
                        candidate,
                        request.BranchId,
                        ct),
                maxLength: 64,
                cancellationToken);

        existing.CompanyId = request.CompanyId;
        existing.BranchCode = branchCode;
        existing.BranchName = request.BranchName.Trim();
        existing.Description = request.Description?.Trim();
        existing.Address = request.Address?.Trim();
        existing.Latitude = request.Latitude;
        existing.Longitude = request.Longitude;
        existing.IsActive = request.IsActive;

        var rows = await _branchRepository.UpdateAsync(existing, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.Conflict("Şube güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _branchRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan şube bulunamadı veya zaten pasif.");
        }

        EnsureBranchAccess(existing);

        var rows = await _branchRepository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan şube bulunamadı veya zaten pasif.");
        }

        return ServiceResult.NoContent();
    }

    private IEnumerable<Branches> FilterAccessible(IEnumerable<Branches> branches)
    {
        if (_companyContext.IsGlobalAdmin)
        {
            return branches;
        }

        return branches.Where(branch =>
            branch.CompanyId.HasValue
            && _companyContext.CanAccessCompany(branch.CompanyId.Value));
    }

    private void EnsureBranchAccess(Branches branch)
    {
        if (branch.CompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(branch.CompanyId.Value);
        }
        else if (!_companyContext.IsGlobalAdmin)
        {
            throw new Application.Exceptions.ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }
    }

    private static BranchDto ToDto(Branches entity) => new()
    {
        BranchId = entity.BranchId,
        CompanyId = entity.CompanyId,
        BranchCode = entity.BranchCode,
        BranchName = entity.BranchName,
        Description = entity.Description,
        Address = entity.Address,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
