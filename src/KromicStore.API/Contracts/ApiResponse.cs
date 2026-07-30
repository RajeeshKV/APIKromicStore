namespace KromicStore.API.Contracts;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message, IReadOnlyCollection<string> Errors, string? TraceId)
{
    public static ApiResponse<T> Ok(T data, string? message, string? traceId) => new(true, data, message, [], traceId);
}
