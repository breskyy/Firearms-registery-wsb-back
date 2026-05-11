namespace EWeaponRegistry.Application.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected AppException(string message, int statusCode, string errorCode) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message, 400, "ValidationError")
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message, 401, "Unauthorized")
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Access denied") : base(message, 403, "Forbidden")
    {
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404, "NotFound")
    {
    }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found", 404, "NotFound")
    {
    }
}

public class BusinessRuleViolationException : AppException
{
    public BusinessRuleViolationException(string message) : base(message, 409, "BusinessRuleViolation")
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409, "Conflict")
    {
    }
}
