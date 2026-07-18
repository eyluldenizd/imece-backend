using System.Diagnostics;
using ImeceWebAPI.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Errors;

/// <summary>
/// Tüm hata response'larını (hem merkezî exception handler hem de
/// ServiceResult tabanlı controller akışı) tek bir RFC 7807 uyumlu
/// ProblemDetails formatında üretmek için merkezî fabrika. Böylece proje
/// genelinde tek ve tutarlı bir hata sözleşmesi olur.
/// </summary>
public static class ImeceProblemDetailsFactory
{
    public static ProblemDetails Create(
        HttpContext httpContext,
        int statusCode,
        string errorCode,
        string title,
        string? detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var options = httpContext.RequestServices
            .GetService<IOptions<ExceptionHandlingOptions>>()
            ?.Value
            ?? new ExceptionHandlingOptions();

        var problemDetails = new ProblemDetails
        {
            Type = ToTypeToken(errorCode),
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path.Value
        };

        problemDetails.Extensions["errorCode"] = errorCode;

        if (options.IncludeTraceId)
        {
            problemDetails.Extensions["traceId"] =
                Activity.Current?.Id ?? httpContext.TraceIdentifier;
        }

        if (errors is { Count: > 0 })
        {
            problemDetails.Extensions["errors"] = errors;
        }

        return problemDetails;
    }

    /// <summary>
    /// "VALIDATION_ERROR" → "validation_error" gibi makine tarafından
    /// okunabilir, tutarlı bir <c>type</c> token'ı üretir.
    /// </summary>
    private static string ToTypeToken(string errorCode) =>
        errorCode.ToLowerInvariant();

    /// <summary>
    /// Hata kodu için sabit, dile bağımlı olmayan başlık döndürür.
    /// </summary>
    public static string TitleFor(string errorCode) => errorCode switch
    {
        Application.Exceptions.ErrorCodes.Validation => "Validation failed",
        Application.Exceptions.ErrorCodes.NotFound => "Resource not found",
        Application.Exceptions.ErrorCodes.Conflict => "Conflict",
        Application.Exceptions.ErrorCodes.BusinessRule => "Business rule violation",
        Application.Exceptions.ErrorCodes.Unauthorized => "Unauthorized",
        Application.Exceptions.ErrorCodes.Forbidden => "Forbidden",
        Application.Exceptions.ErrorCodes.FileValidation => "File validation failed",
        Application.Exceptions.ErrorCodes.Database => "Database error",
        Application.Exceptions.ErrorCodes.BadRequest => "Bad request",
        _ => "Internal server error"
    };
}
