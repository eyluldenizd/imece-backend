namespace Application.Exceptions;

/// <summary>
/// Beklenen uygulama hatalarının ortak tabanı. Global exception handler bu
/// tipteki hataları güvenli ProblemDetails yanıtına çevirir.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(
        string message,
        string errorCode,
        int statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Errors = errors;
    }

    public string ErrorCode { get; }

    public int StatusCode { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
