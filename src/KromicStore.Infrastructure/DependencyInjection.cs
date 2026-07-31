using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Features.Promotions.Abstractions;
using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Taxes.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Persistence.Repositories;
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
        
        // Tenancy services
        services.AddScoped<IReservedSubdomainService, ReservedSubdomainService>();
        
        // Tenant repositories
        services.AddScoped<ITenantRepository,           TenantRepository>();
        
        // Catalog repositories
        services.AddScoped<ICategoryRepository,         CategoryRepository>();
        services.AddScoped<IProductRepository,          ProductRepository>();
        services.AddScoped<ICollectionRepository,       ProductCollectionRepository>();
        
        // Shopping repositories
        services.AddScoped<ICartRepository,             CartRepository>();
        services.AddScoped<IWishlistRepository,         WishlistRepository>();
        services.AddScoped<ICheckoutSessionRepository,  CheckoutSessionRepository>();
        
        // Orders and Payments repositories
        services.AddScoped<IOrderRepository,            OrderRepository>();
        services.AddScoped<IPaymentRepository,          PaymentRepository>();
        
        // Shipping repositories
        services.AddScoped<IShippingZoneRepository,     ShippingZoneRepository>();
        services.AddScoped<IShippingMethodRepository,   ShippingMethodRepository>();
        
        // Tax repositories
        services.AddScoped<ITaxRegionRepository,        TaxRegionRepository>();
        
        // Promotions repositories
        services.AddScoped<IPromotionRepository,        PromotionRepository>();
    }
}
