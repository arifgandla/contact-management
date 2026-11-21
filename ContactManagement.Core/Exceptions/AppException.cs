namespace ContactManagement.Core.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyList<string> Errors { get; }

    public AppException(string message, int statusCode = 500, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? Array.Empty<string>();
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public sealed class ValidationException : AppException
{
    public ValidationException(string message, IReadOnlyList<string>? errors = null)
        : base(message, 400, errors) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have access to this resource.") : base(message, 403) { }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.") : base(message, 401) { }
}
