using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;



namespace Application.Services;



public sealed class SocialActivityService

{

    private static readonly HashSet<string> AllowedStatuses =

        new(StringComparer.OrdinalIgnoreCase) { "Draft", "Published", "Cancelled" };



    private readonly SocialActivityRepository _repository;

    private readonly OrganizationScopeService _organizationScopeService;

    private readonly ICurrentUser _currentUser;



    public SocialActivityService(

        SocialActivityRepository repository,

        OrganizationScopeService organizationScopeService,

        ICurrentUser currentUser)

    {

        _repository = repository;

        _organizationScopeService = organizationScopeService;

        _currentUser = currentUser;

    }



    public async Task<ServiceResult<IReadOnlyList<SocialActivityDto>>> GetAllAsync(

        CancellationToken cancellationToken = default)

    {

        var list = await _repository.GetAllAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<SocialActivityDto>>.Success(list.Select(ToDto).ToList());

    }



    public async Task<ServiceResult<SocialActivityDto>> GetByIdAsync(

        IdRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)

        {

            return ServiceResult<SocialActivityDto>.NotFound("Sosyal aktivite bulunamadı.");

        }



        return ServiceResult<SocialActivityDto>.Success(ToDto(entity));

    }



    public async Task<ServiceResult<long>> CreateAsync(

        CreateSocialActivityDto request,

        CancellationToken cancellationToken = default)

    {

        if (request.StartAt >= request.EndAt)

        {

            return ServiceResult<long>.BadRequest("Başlangıç zamanı bitişten önce olmalıdır.");

        }



        if (!AllowedStatuses.Contains(request.Status))

        {

            return ServiceResult<long>.BadRequest("Geçersiz durum değeri.");

        }



        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);

        if (scopeResult.ErrorMessage is not null)

        {

            return ServiceResult<long>.BadRequest(scopeResult.ErrorMessage);

        }



        var entity = new SocialActivities

        {

            Title = request.Title.Trim(),

            Description = request.Description,

            ActivityType = request.ActivityType.Trim(),

            Location = NormalizeOptional(request.Location),

            StartAt = request.StartAt,

            EndAt = request.EndAt,

            ImageUrl = NormalizeOptional(request.ImageUrl),

            Status = request.Status,

            IsActive = true,

            CreatedBy = _currentUser.GetRequiredUserId()

        };



        ApplyScope(entity, scopeResult.Resolved!);



        var id = await _repository.CreateAsync(entity, cancellationToken);

        return ServiceResult<long>.Created(id);

    }



    public async Task<ServiceResult> UpdateAsync(

        UpdateSocialActivityDto request,

        CancellationToken cancellationToken = default)

    {

        var existing = await _repository.GetByIdAsync(request.SocialActivityId, cancellationToken);

        if (existing is null)

        {

            return ServiceResult.NotFound("Sosyal aktivite bulunamadı.");

        }



        if (request.StartAt >= request.EndAt)

        {

            return ServiceResult.BadRequest("Başlangıç zamanı bitişten önce olmalıdır.");

        }



        if (!AllowedStatuses.Contains(request.Status))

        {

            return ServiceResult.BadRequest("Geçersiz durum değeri.");

        }



        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);

        if (scopeResult.ErrorMessage is not null)

        {

            return ServiceResult.BadRequest(scopeResult.ErrorMessage);

        }



        var entity = new SocialActivities

        {

            SocialActivityId = request.SocialActivityId,

            Title = request.Title.Trim(),

            Description = request.Description,

            ActivityType = request.ActivityType.Trim(),

            Location = NormalizeOptional(request.Location),

            StartAt = request.StartAt,

            EndAt = request.EndAt,

            ImageUrl = NormalizeOptional(request.ImageUrl),

            Status = request.Status,

            IsActive = request.IsActive

        };

        ApplyScope(entity, scopeResult.Resolved!);



        await _repository.UpdateAsync(entity, cancellationToken);

        return ServiceResult.NoContent();

    }



    public async Task<ServiceResult> DeleteAsync(

        IdRequest request,

        CancellationToken cancellationToken = default)

    {

        var rows = await _repository.SoftDeleteAsync(request.Id, cancellationToken);

        if (rows == 0)

        {

            return ServiceResult.NotFound("Sosyal aktivite bulunamadı.");

        }



        return ServiceResult.NoContent();

    }



    private static void ApplyScope(SocialActivities entity, ResolvedOrganizationScope resolved)

    {

        OrganizationScopeService.ApplyToEntity(

            resolved,

            (companyScope, companyId, branchScope, branchId, departmentScope, departmentId) =>

            {

                entity.CompanyScope = companyScope;

                entity.CompanyId = companyId;

                entity.BranchScope = branchScope;

                entity.BranchId = branchId;

                entity.DepartmentScope = departmentScope;

                entity.DepartmentId = departmentId;

            });

    }



    private static string? NormalizeOptional(string? value) =>

        string.IsNullOrWhiteSpace(value) ? null : value.Trim();



    private static SocialActivityDto ToDto(SocialActivityListItem entity) => new()

    {

        SocialActivityId = entity.SocialActivityId,

        Title = entity.Title,

        Description = entity.Description,

        ActivityType = entity.ActivityType,

        Location = entity.Location,

        StartAt = entity.StartAt,

        EndAt = entity.EndAt,

        ImageUrl = entity.ImageUrl,

        CompanyScope = entity.CompanyScope,

        CompanyId = entity.CompanyId,

        BranchScope = entity.BranchScope,

        BranchId = entity.BranchId,

        DepartmentScope = entity.DepartmentScope,

        DepartmentId = entity.DepartmentId,

        Status = entity.Status,

        IsActive = entity.IsActive,

        CreatedBy = entity.CreatedBy,

        CreatedAt = entity.CreatedAt,

        UpdatedAt = entity.UpdatedAt,

        CompanyName = entity.CompanyName,

        BranchName = entity.BranchName,

        DepartmentName = entity.DepartmentName,

        ScopeLabel = OrganizationScopeLabelFormatter.Format(

            entity.CompanyScope,

            entity.CompanyName,

            entity.BranchScope,

            entity.BranchName,

            entity.DepartmentScope,

            entity.DepartmentName)

    };

}


