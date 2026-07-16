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
}
