using Application.Exceptions;
using Core.Common;
using ImeceWebAPI.Errors;
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
                    ErrorCodes.BadRequest,
                    result.Message),

            StatusCodeEnum.NotFound =>
                CreateProblemResult(
                    StatusCodes.Status404NotFound,
                    ErrorCodes.NotFound,
                    result.Message),

            StatusCodeEnum.Conflict =>
                CreateProblemResult(
                    StatusCodes.Status409Conflict,
                    ErrorCodes.Conflict,
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
                    ErrorCodes.BadRequest,
                    result.Message),

            StatusCodeEnum.NotFound =>
                CreateProblemResult(
                    StatusCodes.Status404NotFound,
                    ErrorCodes.NotFound,
                    result.Message),

            StatusCodeEnum.Conflict =>
                CreateProblemResult(
                    StatusCodes.Status409Conflict,
                    ErrorCodes.Conflict,
                    result.Message),

            _ => throw new InvalidOperationException(
                $"Desteklenmeyen durum kodu: {result.StatusCode}")
        };
    }

    private ObjectResult CreateProblemResult(
        int statusCode,
        string errorCode,
        string? detail)
    {
        // ServiceResult tabanlı bilinen hatalar da merkezî exception handler
        // ile aynı ProblemDetails formatında (type, errorCode, traceId,
        // instance) üretilir; böylece tek bir hata sözleşmesi korunur.
        var problemDetails = ImeceProblemDetailsFactory.Create(
            HttpContext,
            statusCode,
            errorCode,
            ImeceProblemDetailsFactory.TitleFor(errorCode),
            detail);

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}