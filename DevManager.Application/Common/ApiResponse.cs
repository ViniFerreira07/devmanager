namespace DevManager.Application.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully.") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? new List<string>()
    };
}

public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Code { get; set; }

    public static ApiResponse Ok(string message = "Operation completed successfully.") => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string message, string? code = null, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Code = code,
        Errors = errors ?? new List<string>()
    };

    public static ApiResponse Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors ?? new List<string>()
    };
}