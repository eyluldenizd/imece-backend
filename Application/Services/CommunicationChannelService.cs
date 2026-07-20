using Application.DTOs;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CommunicationChannelService
{
    private readonly CommunicationChannelsRepository _repository;

    public CommunicationChannelService(CommunicationChannelsRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<CommunicationChannelDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var dtos = list.Select(x => new CommunicationChannelDto
        {
            ChannelId = x.ChannelId,
            ChannelName = x.ChannelName,
            Type = x.Type,
            AddressUrl = x.AddressUrl,
            DepartmentInCharge = x.DepartmentInCharge
        }).ToList();

        return ServiceResult<IReadOnlyList<CommunicationChannelDto>>.Success(dtos);
    }

    public async Task<ServiceResult<CommunicationChannelDto>> GetByIdAsync(IdRequest request,CancellationToken token=default){var x=await _repository.GetByIdAsync(request.Id,token);return x is null?ServiceResult<CommunicationChannelDto>.NotFound("İletişim kanalı bulunamadı."):ServiceResult<CommunicationChannelDto>.Success(ToDto(x));}
    public async Task<ServiceResult<long>> CreateAsync(CreateCommunicationChannelDto r,CancellationToken t=default){var id=await _repository.CreateAsync(new(){ChannelName=r.ChannelName,Type=r.Type,AddressUrl=r.AddressUrl,DepartmentInCharge=r.DepartmentInCharge},t);return ServiceResult<long>.Created(id);}
    public async Task<ServiceResult> UpdateAsync(UpdateCommunicationChannelDto r,CancellationToken t=default){var n=await _repository.UpdateAsync(new(){ChannelId=r.ChannelId,ChannelName=r.ChannelName,Type=r.Type,AddressUrl=r.AddressUrl,DepartmentInCharge=r.DepartmentInCharge},t);return n==0?ServiceResult.NotFound("İletişim kanalı bulunamadı."):ServiceResult.NoContent();}
    public async Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default){var n=await _repository.DeleteAsync(r.Id,t);return n==0?ServiceResult.NotFound("İletişim kanalı bulunamadı."):ServiceResult.NoContent();}
    private static CommunicationChannelDto ToDto(Core.Entities.CommunicationChannels x)=>new(){ChannelId=x.ChannelId,ChannelName=x.ChannelName,Type=x.Type,AddressUrl=x.AddressUrl,DepartmentInCharge=x.DepartmentInCharge};
}
