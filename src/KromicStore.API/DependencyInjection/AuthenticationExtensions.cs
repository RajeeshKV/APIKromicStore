using KromicStore.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KromicStore.API.DependencyInjection;

/// <summary>
/// JWT Bearer authentication configuration.
/// Reads from Jwt section and validates on startup.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        ValidateOnStartup(jwtOptions);

        services.Configure<JwtOptions>(jwtSection);

        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Secret);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtOptions.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = jwtOptions.Audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                            ctx.Response.Headers.Append("X-Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static void ValidateOnStartup(JwtOptions options)
    {
        var ctx     = new ValidationContext(options);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(options, ctx, results, validateAllProperties: true))
        {
            var msg = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException($"Jwt configuration is invalid: {msg}");
        }
    }
}
