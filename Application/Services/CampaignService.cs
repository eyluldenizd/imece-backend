using Application.DTOs;
using Core.Common;
using Core.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CampaignService
{
    private readonly CampaignsRepository _campaignsRepository;

    public CampaignService(CampaignsRepository campaignsRepository)
    {
        _campaignsRepository = campaignsRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _campaignsRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CampaignDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await _campaignsRepository.GetActiveAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CampaignDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<CampaignDto>> GetByIdAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _campaignsRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return ServiceResult<CampaignDto>.NotFound("Kampanya bulunamadı.");

        return ServiceResult<CampaignDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(CreateCampaignDto request, CancellationToken cancellationToken = default)
    {
        var entity = new Campaigns
        {
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            TargetUrl = request.TargetUrl,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        var id = await _campaignsRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateCampaignDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _campaignsRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (entity is null)
            return ServiceResult.NotFound("Kampanya bulunamadı.");

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.ImageUrl = request.ImageUrl;
        entity.TargetUrl = request.TargetUrl;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.IsActive = request.IsActive;

        await _campaignsRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        await _campaignsRepository.DeleteAsync(request.Id, cancellationToken);
        return ServiceResult.NoContent();
    }

    private static CampaignDto ToDto(Campaigns entity) => new()
    {
        CampaignId = entity.CampaignId,
        Title = entity.Title,
        Description = entity.Description,
        ImageUrl = entity.ImageUrl,
        TargetUrl = entity.TargetUrl,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        IsActive = entity.IsActive
    };
}
