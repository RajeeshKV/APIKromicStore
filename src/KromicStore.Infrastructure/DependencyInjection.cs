using KromicStore.Application.Common.Abstractions;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Services;
using KromicStore.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KromicStore.Infrastructure;

/// <summary>
/// Registers all Infrastructure services in a single extension method.
/// Called once from Program.cs via AddInfrastructure().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddJwt(services, configuration);
        AddServices(services);
        return services;
    }

    // ── Persistence ─────────────────────────────────────────────────────────

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        services.AddDbContext<KromicStoreDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<KromicStoreDbContext>());
    }

    // ── JWT (options only — bearer middleware configured in API layer) ───────

    private static void AddJwt(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
    }

    // ── Application services ─────────────────────────────────────────────────

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher,     PasswordHasher>();
        services.AddScoped<ITokenService,       TokenService>();
    }
}
