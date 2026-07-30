using KromicStore.API.Middleware;

namespace KromicStore.API.DependencyInjection;

/// <summary>
/// Middleware registration extensions.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds all required middlewares to the application pipeline in the correct order.
    /// Order matters: exception handling first, then specific handlers.
    /// </summary>
    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        // Exception handling must be first
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Tenant resolution must come before authentication
        app.UseMiddleware<TenantResolutionMiddleware>();

        // HTTPS redirection
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
