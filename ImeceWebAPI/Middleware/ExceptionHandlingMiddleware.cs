using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ImeceWebAPI.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    "Doğrulama Hatası",
                    ex.Message,
                    ex.Errors);
            }
            catch (NotFoundException ex)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.NotFound,
                    "Kayıt Bulunamadı",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen bir hata oluştu. Path: {Path}", context.Request.Path);
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Sunucu Hatası",
                    "Beklenmeyen bir hata oluştu.");
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            string detail,
            IReadOnlyDictionary<string, string[]>? errors = null)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new
            {
                title,
                status = (int)statusCode,
                detail,
                errors
            };

            var json = JsonSerializer.Serialize(problem, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}