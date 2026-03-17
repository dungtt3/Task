namespace TaskManager.Shared.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message)
    {
    }
}
