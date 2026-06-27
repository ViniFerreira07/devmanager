namespace DevManager.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Code = "VALIDATION_ERROR";
        Errors = new List<string> { message };
    }

    public ValidationException(string code, string message)
        : base(message)
    {
        Code = code;
        Errors = new List<string> { message };
    }

    public ValidationException(IReadOnlyList<string> errors)
        : base(string.Join("; ", errors))
    {
        Code = "VALIDATION_ERROR";
        Errors = errors;
    }
}