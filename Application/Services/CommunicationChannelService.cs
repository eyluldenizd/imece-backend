using Application.Common.OrganizationScope;

using Application.DTOs;

using Core.Common;

using Core.Entities;

using Infrastructure.Repositories;



namespace Application.Services;



public sealed class CommunicationChannelService

{

    private readonly CommunicationChannelsRepository _repository;

    private readonly CommunicationChannelTypeRepository _typeRepository;

    private readonly OrganizationScopeService _organizationScopeService;



    public CommunicationChannelService(

        CommunicationChannelsRepository repository,

        CommunicationChannelTypeRepository typeRepository,

        OrganizationScopeService organizationScopeService)

    {

        _repository = repository;

        _typeRepository = typeRepository;

        _organizationScopeService = organizationScopeService;

    }



    public async Task<ServiceResult<IReadOnlyList<CommunicationChannelDto>>> GetAllAsync(

        CancellationToken cancellationToken = default)

    {

        var list = await _repository.GetAllAsync(cancellationToken);

        return ServiceResult<IReadOnlyList<CommunicationChannelDto>>.Success(

            list.Select(ToDto).ToList());

    }



    public async Task<ServiceResult<CommunicationChannelDto>> GetByIdAsync(

        IdRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)

        {

            return ServiceResult<CommunicationChannelDto>.NotFound("İletişim kanalı bulunamadı.");

        }



        return ServiceResult<CommunicationChannelDto>.Success(ToDto(entity));

    }



    public async Task<ServiceResult<long>> CreateAsync(

        CreateCommunicationChannelDto request,

        CancellationToken cancellationToken = default)

    {

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);

        if (scopeResult.ErrorMessage is not null)

        {

            return ServiceResult<long>.BadRequest(scopeResult.ErrorMessage);

        }



        var typeName = await ResolveTypeNameAsync(
            request.CommunicationChannelTypeId,
            request.Type,
            cancellationToken);

        if (typeName.ErrorMessage is not null)

        {

            return ServiceResult<long>.BadRequest(typeName.ErrorMessage);

        }



        var entity = new CommunicationChannels

        {

            ChannelName = request.ChannelName.Trim(),

            Type = typeName.Name!,

            CommunicationChannelTypeId = request.CommunicationChannelTypeId,

            AddressUrl = request.AddressUrl.Trim(),

            DepartmentInCharge = NormalizeOptional(request.DepartmentInCharge),

            Description = request.Description,

            Icon = NormalizeOptional(request.Icon),

            SortOrder = request.SortOrder,

            IsActive = true

        };



        ApplyScope(entity, scopeResult.Resolved!);



        var id = await _repository.CreateAsync(entity, cancellationToken);

        return ServiceResult<long>.Created(id);

    }



    public async Task<ServiceResult> UpdateAsync(

        UpdateCommunicationChannelDto request,

        CancellationToken cancellationToken = default)

    {

        var entity = await _repository.GetByIdAsync(request.ChannelId, cancellationToken);

        if (entity is null)

        {

            return ServiceResult.NotFound("İletişim kanalı bulunamadı.");

        }



        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);

        if (scopeResult.ErrorMessage is not null)

        {

            return ServiceResult.BadRequest(scopeResult.ErrorMessage);

        }



        var typeName = await ResolveTypeNameAsync(
            request.CommunicationChannelTypeId,
            request.Type,
            cancellationToken);

        if (typeName.ErrorMessage is not null)

        {

            return ServiceResult.BadRequest(typeName.ErrorMessage);

        }



        entity.ChannelName = request.ChannelName.Trim();

        entity.Type = typeName.Name!;

        entity.CommunicationChannelTypeId = request.CommunicationChannelTypeId;

        entity.AddressUrl = request.AddressUrl.Trim();

        entity.DepartmentInCharge = NormalizeOptional(request.DepartmentInCharge);

        entity.Description = request.Description;

        entity.Icon = NormalizeOptional(request.Icon);

        entity.SortOrder = request.SortOrder;

        entity.IsActive = request.IsActive;

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

            return ServiceResult.NotFound("İletişim kanalı bulunamadı.");

        }



        return ServiceResult.NoContent();

    }



    private async Task<(string? Name, string? ErrorMessage)> ResolveTypeNameAsync(
        int? communicationChannelTypeId,
        string? type,
        CancellationToken cancellationToken)
    {
        if (communicationChannelTypeId.HasValue)
        {
            var channelType = await _typeRepository.GetByIdAsync(
                communicationChannelTypeId.Value,
                cancellationToken);

            if (channelType is null)
            {
                return (null, "İletişim kanalı türü bulunamadı.");
            }

            return (channelType.Name, null);
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            return (null, "Kanal türü zorunludur.");
        }

        return (type.Trim(), null);
    }



    private static void ApplyScope(CommunicationChannels entity, ResolvedOrganizationScope resolved)

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



    private static CommunicationChannelDto ToDto(CommunicationChannels entity) => new()

    {

        ChannelId = entity.ChannelId,

        ChannelName = entity.ChannelName,

        Type = entity.Type,

        CommunicationChannelTypeId = entity.CommunicationChannelTypeId,

        AddressUrl = entity.AddressUrl,

        DepartmentInCharge = entity.DepartmentInCharge,

        Description = entity.Description,

        Icon = entity.Icon,

        SortOrder = entity.SortOrder,

        IsActive = entity.IsActive,

        CompanyScope = entity.CompanyScope,

        CompanyId = entity.CompanyId,

        BranchScope = entity.BranchScope,

        BranchId = entity.BranchId,

        DepartmentScope = entity.DepartmentScope,

        DepartmentId = entity.DepartmentId,

        CompanyName = entity.CompanyName,

        BranchName = entity.BranchName,

        DepartmentName = entity.DepartmentName,

        TypeIconUrl = entity.TypeIconUrl,

        CreatedAt = entity.CreatedAt,

        UpdatedAt = entity.UpdatedAt

    };

}


