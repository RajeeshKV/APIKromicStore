using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Tenants;
using KromicStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Tests;

public sealed class KromicStoreDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldPopulateAuditFields()
    {
        var tenantId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);
        var settings = TenantSettings.CreateDefault(tenantId);

        context.AddEntity(settings);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, settings.CreatedOnUtc);
        Assert.Equal("System", settings.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldConvertDeleteToSoftDelete()
    {
        var tenantId = Guid.NewGuid();
        await using var context = CreateContext(tenantId);
        var settings = TenantSettings.CreateDefault(tenantId);
        context.AddEntity(settings);
        await context.SaveChangesAsync();

        context.Remove(settings);
        await context.SaveChangesAsync();

        Assert.True(settings.IsDeleted);
        Assert.NotNull(settings.DeletedOnUtc);
        Assert.Empty(await context.TenantSettings.ToListAsync());
    }

    [Fact]
    public async Task TenantOwnedQueryFilters_ShouldRestrictByCurrentTenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var databaseName = Guid.NewGuid().ToString("N");

        await using (var seedContext = CreateContext(tenantA, databaseName))
        {
            seedContext.AddEntity(TenantSettings.CreateDefault(tenantA));
            seedContext.AddEntity(TenantSettings.CreateDefault(tenantB));
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(tenantA, databaseName);

        var settings = await context.TenantSettings.ToListAsync();

        Assert.Single(settings);
        Assert.Equal(tenantA, settings[0].TenantId);
    }

    private static KromicStoreDbContext CreateContext(Guid? tenantId, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<KromicStoreDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new KromicStoreDbContext(options, new TestTenantContext(tenantId));
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid? tenantId)
        {
            TenantId = tenantId;
        }

        public Guid? TenantId { get; }
        public Guid? StoreId => null;
        public string? StoreName => null;
        public string? Locale => null;
        public string? TimeZone => null;
        public bool IsResolved => TenantId.HasValue;
    }
}
