using KromicStore.Infrastructure.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace KromicStore.API.DependencyInjection;

/// <summary>
/// Extensions for CORS configuration with wildcard pattern support.
/// Handles both exact origins and wildcard patterns like "https://*.kromic.in"
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Configures CORS with support for wildcard patterns and credentials.
    /// 
    /// Wildcard patterns like "https://*.kromic.in" are expanded to match any subdomain.
    /// This is necessary because ASP.NET Core CORS middleware doesn't natively support
    /// wildcard patterns with credentials.
    /// </summary>
    public static IServiceCollection AddWildcardCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Infrastructure.Configuration.CorsOptions>(configuration.GetSection(Infrastructure.Configuration.CorsOptions.SectionName));
        
        services.AddCors(options =>
        {
            var corsOptions = new Infrastructure.Configuration.CorsOptions();
            configuration.GetSection(Infrastructure.Configuration.CorsOptions.SectionName).Bind(corsOptions);
            
            var (isValid, errorMessage) = corsOptions.Validate();
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid CORS configuration: {errorMessage}");
            }

            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin => corsOptions.IsOriginAllowed(origin))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}

