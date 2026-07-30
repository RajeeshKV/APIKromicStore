using KromicStore.Application.Common.Abstractions;
using KromicStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Application.Tests.Common;

/// <summary>
/// Creates a fresh isolated in-memory DbContext for each handler test.
/// Uses InMemory provider — sufficient for logic testing.
/// </summary>
public static class InMemoryDbContextFactory
{
    public static KromicStoreDbContext Create(Guid? tenantId = null)
    {
        var options = new DbContextOptionsBuilder<KromicStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new KromicStoreDbContext(options, new TestTenantContext(tenantId));
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid? tenantId) => TenantId = tenantId;
        public Guid?   TenantId  { get; }
        public Guid?   StoreId   => null;
        public string? StoreName => null;
        public string? Locale    => null;
        public string? TimeZone  => null;
        public bool    IsResolved => TenantId.HasValue;
    }
}
