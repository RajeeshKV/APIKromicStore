using KromicStore.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KromicStore.Infrastructure.Persistence;

/// <summary>
/// Factory for creating DbContext instances during design-time (migrations).
/// This allows EF Core to create migrations without needing the full DI container.
/// </summary>
public sealed class KromicStoreDbContextFactory : IDesignTimeDbContextFactory<KromicStoreDbContext>
{
    public KromicStoreDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../KromicStore.API"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Get database options
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

        var optionsBuilder = new DbContextOptionsBuilder<KromicStoreDbContext>();
        
        // Use PostgreSQL
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        // Create a mock TenantContext that allows all data (for migrations)
        var tenantContext = new NoOpTenantContext();

        return new KromicStoreDbContext(optionsBuilder.Options, tenantContext);
    }

    /// <summary>
    /// No-op tenant context for use during migrations.
    /// Allows all queries to execute without tenant filtering.
    /// </summary>
    private sealed class NoOpTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public string? StoreName => null;
        public Guid? StoreId => null;
        public string? Locale => "en-US";
        public string? TimeZone => "UTC";
        public bool IsResolved => false;

        public void Set(Guid tenantId, string? storeName = null) { }
        public void Reset() { }
    }
}
