using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Features.Email.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Features.Promotions.Abstractions;
using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Taxes.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Infrastructure.BackgroundJobs;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Health;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Persistence.Repositories;
using KromicStore.Infrastructure.Services;
using KromicStore.Infrastructure.Services.Email;
using KromicStore.Infrastructure.Services.Media;
using KromicStore.Infrastructure.Services.Payments;
using KromicStore.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
        AddPlatformConfiguration(services, configuration);
        
        // Register HttpClientFactory first
        services.AddHttpClient();
        
        ConfigureHttpClients(services, configuration);
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

    // ── Platform Configuration ───────────────────────────────────────────────

    private static void AddPlatformConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        // Configure multi-tenancy options
        services.Configure<MultiTenancyOptions>(configuration.GetSection(MultiTenancyOptions.SectionName));

        // Configure CORS options
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

        // Configure external service options
        services.Configure<BrevoOptions>(configuration.GetSection(BrevoOptions.SectionName));
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.Configure<RazorpayOptions>(configuration.GetSection(RazorpayOptions.SectionName));

        // Register configuration validator
        services.AddSingleton<PlatformConfigurationValidator>();
    }

    // ── Application services ─────────────────────────────────────────────────

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher,     PasswordHasher>();
        services.AddScoped<ITokenService,       TokenService>();
        
        // External services
        services.AddScoped<Infrastructure.Services.Media.IMediaService, CloudinaryMediaService>();
        services.AddScoped<IEmailService, BrevoEmailService>();
        services.AddScoped<IPaymentGateway, RazorpayPaymentGateway>();
        
        // Email services
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();
        services.AddScoped<EmailOutboxProcessor>();
        
        // Background jobs (no Hangfire - using hosted services)
        services.AddHostedService<EmailOutboxBackgroundWorker>();
        
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
        
        // Health check services
        services.AddSingleton<ApplicationStartupState>();
        services.AddScoped<IHealthCheckService, Health.HealthCheckService>();
    }

    // ── HTTP Client Configuration ─────────────────────────────────────────────

    private static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        var brevoOptions = configuration.GetSection(BrevoOptions.SectionName).Get<BrevoOptions>();
        var cloudinaryOptions = configuration.GetSection(CloudinaryOptions.SectionName).Get<CloudinaryOptions>();
        var razorpayOptions = configuration.GetSection(RazorpayOptions.SectionName).Get<RazorpayOptions>();

        // Configure Brevo HttpClient
        if (brevoOptions?.Enabled ?? false)
        {
            services.AddHttpClient("Brevo", client =>
            {
                client.BaseAddress = new Uri(brevoOptions.BaseUrl);
                client.DefaultRequestHeaders.Add("api-key", brevoOptions.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(brevoOptions.RequestTimeoutSeconds);
            });
        }

        // Configure Cloudinary HttpClient
        if (cloudinaryOptions?.Enabled ?? false)
        {
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{cloudinaryOptions.ApiKey}:{cloudinaryOptions.ApiSecret}"
                )
            );

            services.AddHttpClient("Cloudinary", client =>
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                client.Timeout = TimeSpan.FromSeconds(cloudinaryOptions.RequestTimeoutSeconds);
            });
        }

        // Configure Razorpay HttpClient
        if (razorpayOptions?.Enabled ?? false)
        {
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{razorpayOptions.KeyId}:{razorpayOptions.KeySecret}"
                )
            );

            services.AddHttpClient("Razorpay", client =>
            {
                client.BaseAddress = new Uri(razorpayOptions.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(razorpayOptions.RequestTimeoutSeconds);
            });
        }
    }
}
