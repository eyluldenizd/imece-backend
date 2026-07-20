using Application.Common.Codes;
using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class MeetingRoomService
{
    private readonly MeetingRoomRepository _repository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;
    private readonly IEntityCodeGenerator _codeGenerator;

    public MeetingRoomService(
        MeetingRoomRepository repository,
        ICompanyContext companyContext,
        ICurrentUser currentUser,
        IEntityCodeGenerator codeGenerator)
    {
        _repository = repository;
        _companyContext = companyContext;
        _currentUser = currentUser;
        _codeGenerator = codeGenerator;
    }

    public async Task<ServiceResult<IReadOnlyList<MeetingRoomDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var list = await _repository.GetAllAsync(filter, cancellationToken);
        return ServiceResult<IReadOnlyList<MeetingRoomDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<MeetingRoomDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<MeetingRoomDto>.NotFound("Toplantı odası bulunamadı.");
        }

        _companyContext.EnsureCanAccessCompany(entity.CompanyId);
        return ServiceResult<MeetingRoomDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateMeetingRoomDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var code = await _codeGenerator.AllocateAsync(
            request.Code,
            request.Name,
            "ROOM",
            async (candidate, ct) =>
                await _repository.ExistsByCodeInCompanyAsync(request.CompanyId, candidate, cancellationToken: ct),
            maxLength: 64,
            cancellationToken);

        var entity = MapEntity(request, code);
        entity.IsActive = true;

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateMeetingRoomDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.MeetingRoomId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Toplantı odası bulunamadı.");
        }

        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? entity.Code
            : await _codeGenerator.AllocateAsync(
                request.Code,
                request.Name,
                "ROOM",
                async (candidate, ct) =>
                    await _repository.ExistsByCodeInCompanyAsync(
                        request.CompanyId,
                        candidate,
                        request.MeetingRoomId,
                        ct),
                maxLength: 64,
                cancellationToken);

        entity.CompanyId = request.CompanyId;
        entity.BranchId = request.BranchId;
        entity.DepartmentId = request.DepartmentId;
        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.Floor = NormalizeOptional(request.Floor);
        entity.Capacity = request.Capacity;
        entity.LocationDescription = NormalizeOptional(request.LocationDescription);
        entity.Features = NormalizeOptional(request.Features);
        entity.IsActive = request.IsActive;

        await _repository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Toplantı odası bulunamadı.");
        }

        _companyContext.EnsureCanAccessCompany(entity.CompanyId);

        var rows = await _repository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Toplantı odası bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static MeetingRooms MapEntity(CreateMeetingRoomDto request, string code) => new()
    {
        CompanyId = request.CompanyId,
        BranchId = request.BranchId,
        DepartmentId = request.DepartmentId,
        Name = request.Name.Trim(),
        Code = code,
        Floor = NormalizeOptional(request.Floor),
        Capacity = request.Capacity,
        LocationDescription = NormalizeOptional(request.LocationDescription),
        Features = NormalizeOptional(request.Features)
    };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static MeetingRoomDto ToDto(MeetingRooms entity) => new()
    {
        MeetingRoomId = entity.MeetingRoomId,
        CompanyId = entity.CompanyId,
        BranchId = entity.BranchId,
        DepartmentId = entity.DepartmentId,
        Name = entity.Name,
        Code = entity.Code,
        Floor = entity.Floor,
        Capacity = entity.Capacity,
        LocationDescription = entity.LocationDescription,
        Features = entity.Features,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
