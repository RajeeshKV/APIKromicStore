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

        // CORS must come before authentication
        app.UseCors("AllowSpecificOrigins");

        // HTTPS redirection
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Authentication must run before TenantResolution so that
        // httpContext.User.Identity.IsAuthenticated is true when the
        // middleware reads the JWT tenantId claim.
        app.UseAuthentication();

        // Tenant resolution runs after authentication so the JWT is
        // already validated and User.Identity.IsAuthenticated == true.
        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();

        return app;
    }
}
