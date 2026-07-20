namespace Application.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message)
        : base(message, ErrorCodes.Unauthorized, 401)
    {
    }
}
