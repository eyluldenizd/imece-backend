namespace Core.Common;

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(
        StatusCodeEnum statusCode,
        T? data = default,
        string? message = null)
        : base(
            statusCode,
            message)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(
        T data)
    {
        return new ServiceResult<T>(
            StatusCodeEnum.Success,
            data);
    }

    public static ServiceResult<T> Created(
        T data)
    {
        return new ServiceResult<T>(
            StatusCodeEnum.Created,
            data);
    }

    public new static ServiceResult<T> BadRequest(
        string message)
    {
        return new ServiceResult<T>(
            StatusCodeEnum.BadRequest,
            message: message);
    }

    public new static ServiceResult<T> NotFound(
        string message)
    {
        return new ServiceResult<T>(
            StatusCodeEnum.NotFound,
            message: message);
    }

    public new static ServiceResult<T> Conflict(
        string message)
    {
        return new ServiceResult<T>(
            StatusCodeEnum.Conflict,
            message: message);
    }
}