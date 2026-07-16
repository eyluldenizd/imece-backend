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
}
