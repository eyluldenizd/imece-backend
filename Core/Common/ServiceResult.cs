namespace Core.Common;

public class ServiceResult
{
    public StatusCodeEnum StatusCode { get; }

    public string? Message { get; }

    protected ServiceResult(
        StatusCodeEnum statusCode,
        string? message = null)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public static ServiceResult Success()
    {
        return new ServiceResult(
            StatusCodeEnum.Success);
    }

    public static ServiceResult NoContent()
    {
        return new ServiceResult(
            StatusCodeEnum.NoContent);
    }

    public static ServiceResult BadRequest(
        string message)
    {
        return new ServiceResult(
            StatusCodeEnum.BadRequest,
            message);
    }

    public static ServiceResult NotFound(
        string message)
    {
        return new ServiceResult(
            StatusCodeEnum.NotFound,
            message);
    }

    public static ServiceResult Conflict(
        string message)
    {
        return new ServiceResult(
            StatusCodeEnum.Conflict,
            message);
    }
}