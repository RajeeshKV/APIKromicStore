namespace KromicStore.Application.Common.Models;

public sealed record Result<T>(bool Success, T? Data, string? Message, IReadOnlyCollection<string> Errors)
{
    public static Result<T> Ok(T data, string? message = null) => new(true, data, message, []);
    public static Result<T> Fail(string message, params string[] errors) => new(false, default, message, errors);
}
