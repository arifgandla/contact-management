namespace ContactManagement.Core.DTOs.Common;

public abstract class ApiResponseBase
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
}

public sealed class ApiResponse : ApiResponseBase
{
    public object? Data { get; init; }

    public static ApiResponse Ok(string message) => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public sealed class ApiResponse<T> : ApiResponseBase
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message) =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
