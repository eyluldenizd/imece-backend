using Application.DTOs;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CorporateAppService
{
    private readonly CorporateAppsRepository _repository;

    public CorporateAppService(CorporateAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<CorporateAppDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var dtos = list.Select(x => new CorporateAppDto
        {
            AppId = x.AppId,
            Title = x.Title,
            Description = x.Description,
            Url = x.Url,
            Category = x.Category
        }).ToList();

        return ServiceResult<IReadOnlyList<CorporateAppDto>>.Success(dtos);
    }

    public async Task<ServiceResult<CorporateAppDto>> GetByIdAsync(IdRequest r,CancellationToken t=default){var x=await _repository.GetByIdAsync(r.Id,t);return x is null?ServiceResult<CorporateAppDto>.NotFound("Kurumsal uygulama bulunamadı."):ServiceResult<CorporateAppDto>.Success(ToDto(x));}
    public async Task<ServiceResult<long>> CreateAsync(CreateCorporateAppDto r,CancellationToken t=default){var id=await _repository.CreateAsync(new(){Title=r.Title,Description=r.Description,Url=r.Url,Category=r.Category},t);return ServiceResult<long>.Created(id);}
    public async Task<ServiceResult> UpdateAsync(UpdateCorporateAppDto r,CancellationToken t=default){var n=await _repository.UpdateAsync(new(){AppId=r.AppId,Title=r.Title,Description=r.Description,Url=r.Url,Category=r.Category},t);return n==0?ServiceResult.NotFound("Kurumsal uygulama bulunamadı."):ServiceResult.NoContent();}
    public async Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default){var n=await _repository.DeleteAsync(r.Id,t);return n==0?ServiceResult.NotFound("Kurumsal uygulama bulunamadı."):ServiceResult.NoContent();}
    private static CorporateAppDto ToDto(Core.Entities.CorporateApps x)=>new(){AppId=x.AppId,Title=x.Title,Description=x.Description,Url=x.Url,Category=x.Category};
}
