namespace DevManager.Application.Common;

public sealed class Result<T>
{
    public bool Success { get; }
    public string Message { get; }
    public T? Data { get; }

    public bool IsSuccess => Success;
    public T? Value => Data;
    public string? Error => Success ? null : Message;

    private Result(bool success, string message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static Result<T> Ok(T data, string message = "Operation completed successfully.") => new(true, message, data);
    public static Result<T> Fail(string message) => new(false, message, default);

    public static Result<T> SuccessResult(T data, string message = "Operation completed successfully.") => Ok(data, message);
    public static Result<T> Failure(string message) => Fail(message);
}
