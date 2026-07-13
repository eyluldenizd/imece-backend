using Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers.Common;

public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken)
    {
        var result = await serviceMethod(
            cancellationToken);

        return ConvertToActionResult(result);
    }

    protected async Task<IActionResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<ServiceResult<TResponse>>>
            serviceMethod,
        CancellationToken cancellationToken)
    {
        var result = await serviceMethod(
            request,
            cancellationToken);

        return ConvertToActionResult(result);
    }

    protected async Task<IActionResult> ExecuteAsync<TRequest>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<ServiceResult>>
            serviceMethod,
        CancellationToken cancellationToken)
    {
        var result = await serviceMethod(
            request,
            cancellationToken);

        return ConvertToActionResult(result);
    }

    protected async Task<IActionResult> ExecuteAsync(
        Func<CancellationToken, Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken)
    {
        var result = await serviceMethod(
            cancellationToken);

        return ConvertToActionResult(result);
    }

    private IActionResult ConvertToActionResult<T>(
        ServiceResult<T> result)
    {
        return result.StatusCode switch
        {
            StatusCodeEnum.Success =>
                Ok(result.Data),

            StatusCodeEnum.Created =>
                StatusCode(
                    StatusCodes.Status201Created,
                    result.Data),

            StatusCodeEnum.NoContent =>
                NoContent(),

            StatusCodeEnum.BadRequest =>
                CreateProblemResult(
                    StatusCodes.Status400BadRequest,
                    "Geçersiz istek",
                    result.Message),

            StatusCodeEnum.NotFound =>
                CreateProblemResult(
                    StatusCodes.Status404NotFound,
                    "Kayıt bulunamadı",
                    result.Message),

            StatusCodeEnum.Conflict =>
                CreateProblemResult(
                    StatusCodes.Status409Conflict,
                    "İşlem çakışması",
                    result.Message),

            _ => throw new InvalidOperationException(
                $"Desteklenmeyen durum kodu: {result.StatusCode}")
        };
    }

    private IActionResult ConvertToActionResult(
        ServiceResult result)
    {
        return result.StatusCode switch
        {
            StatusCodeEnum.Success =>
                Ok(),

            StatusCodeEnum.Created =>
                StatusCode(
                    StatusCodes.Status201Created),

            StatusCodeEnum.NoContent =>
                NoContent(),

            StatusCodeEnum.BadRequest =>
                CreateProblemResult(
                    StatusCodes.Status400BadRequest,
                    "Geçersiz istek",
                    result.Message),

            StatusCodeEnum.NotFound =>
                CreateProblemResult(
                    StatusCodes.Status404NotFound,
                    "Kayıt bulunamadı",
                    result.Message),

            StatusCodeEnum.Conflict =>
                CreateProblemResult(
                    StatusCodes.Status409Conflict,
                    "İşlem çakışması",
                    result.Message),

            _ => throw new InvalidOperationException(
                $"Desteklenmeyen durum kodu: {result.StatusCode}")
        };
    }

    private ObjectResult CreateProblemResult(
        int statusCode,
        string title,
        string? detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        return StatusCode(
            statusCode,
            problemDetails);
    }
}