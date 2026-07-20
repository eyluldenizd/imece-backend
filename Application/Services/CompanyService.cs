using Application.Common.Codes;
using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CompanyService
{
    private readonly CompanyRepository _companyRepository;
    private readonly IEntityCodeGenerator _codeGenerator;

    public CompanyService(
        CompanyRepository companyRepository,
        IEntityCodeGenerator codeGenerator)
    {
        _companyRepository = companyRepository;
        _codeGenerator = codeGenerator;
    }

    public async Task<ServiceResult<IReadOnlyList<CompanyDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CompanyDto>>.Success(
            companies.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CompanyDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetActiveAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CompanyDto>>.Success(
            companies.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<CompanyDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(
            (int)request.Id,
            cancellationToken);

        if (company is null)
        {
            return ServiceResult<CompanyDto>.NotFound(
                $"ID değeri {request.Id} olan şirket bulunamadı.");
        }

        return ServiceResult<CompanyDto>.Success(ToDto(company));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateCompanyDto request,
        CancellationToken cancellationToken = default)
    {
        var companyCode = await _codeGenerator.AllocateAsync(
            request.CompanyCode,
            request.CompanyName,
            "CO",
            async (candidate, ct) =>
                await _companyRepository.ExistsByCodeAsync(candidate, cancellationToken: ct),
            maxLength: 64,
            cancellationToken);

        var entity = new Companies
        {
            CompanyCode = companyCode,
            CompanyName = request.CompanyName.Trim(),
            Description = request.Description?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            IsActive = request.IsActive
        };

        var companyId = await _companyRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(companyId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateCompanyDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _companyRepository.GetByIdAsync(
            request.CompanyId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.CompanyId} olan şirket bulunamadı.");
        }

        var companyCode = string.IsNullOrWhiteSpace(request.CompanyCode)
            ? existing.CompanyCode
            : await _codeGenerator.AllocateAsync(
                request.CompanyCode,
                request.CompanyName,
                "CO",
                async (candidate, ct) =>
                    await _companyRepository.ExistsByCodeAsync(candidate, request.CompanyId, ct),
                maxLength: 64,
                cancellationToken);

        existing.CompanyCode = companyCode;
        existing.CompanyName = request.CompanyName.Trim();
        existing.Description = request.Description?.Trim();
        existing.LogoUrl = request.LogoUrl?.Trim();
        existing.IsActive = request.IsActive;

        var rows = await _companyRepository.UpdateAsync(existing, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.Conflict("Şirket güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = await _companyRepository.SoftDeleteAsync(
            (int)request.Id,
            cancellationToken);

        if (rows == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan şirket bulunamadı veya zaten pasif.");
        }

        return ServiceResult.NoContent();
    }

    private static CompanyDto ToDto(Companies entity) => new()
    {
        CompanyId = entity.CompanyId,
        CompanyCode = entity.CompanyCode,
        CompanyName = entity.CompanyName,
        Description = entity.Description,
        LogoUrl = entity.LogoUrl,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
