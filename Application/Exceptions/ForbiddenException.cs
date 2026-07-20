namespace Application.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(message, ErrorCodes.Forbidden, StatusCodes.Status403Forbidden)
    {
    }
}

// ASP.NET StatusCodes olmadan derlenebilmesi için lokal sabit.
file static class StatusCodes
{
    public const int Status403Forbidden = 403;
}
