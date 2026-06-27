namespace DevManager.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string name, object key)
        : base($"{name} ({key}) was not found.")
    {
        Code = $"{name.ToUpperInvariant()}_NOT_FOUND";
    }

    public NotFoundException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}