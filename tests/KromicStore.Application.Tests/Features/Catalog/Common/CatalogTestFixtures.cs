using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Common;

/// <summary>
/// Common test fixtures and utilities for Catalog tests.
/// </summary>
public static class CatalogTestFixtures
{
    /// <summary>
    /// Creates an in-memory DbContext with a specific tenant.
    /// </summary>
    public static KromicStoreDbContext CreateDbContext(Guid? tenantId = null)
    {
        tenantId ??= Guid.NewGuid();
        
        var options = new DbContextOptionsBuilder<KromicStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new KromicStoreDbContext(options, new TestTenantContext(tenantId.Value));
    }

    /// <summary>
    /// Creates a mock ITenantContext with a specific tenant.
    /// </summary>
    public static ITenantContext CreateTenantContext(Guid? tenantId = null)
    {
        tenantId ??= Guid.NewGuid();
        return new TestTenantContext(tenantId.Value);
    }

    /// <summary>
    /// Creates a mock ICurrentUserService.
    /// </summary>
    public static ICurrentUserService CreateCurrentUserService(Guid? userId = null)
    {
        userId ??= Guid.NewGuid();
        var service = Substitute.For<ICurrentUserService>();
        service.UserId.Returns(userId.Value);
        return service;
    }

    /// <summary>
    /// Creates a test category with default values.
    /// </summary>
    public static Category CreateTestCategory(Guid tenantId, string name = "Test Category")
    {
        var category = Category.Create(
            tenantId: tenantId,
            name: name);
        category.MarkCreated(DateTime.UtcNow, "test-user");
        return category;
    }

    /// <summary>
    /// Creates a test product with default values.
    /// </summary>
    public static Product CreateTestProduct(
        Guid tenantId,
        Guid categoryId,
        string sku = "TEST-001",
        string name = "Test Product")
    {
        var product = Product.Create(
            tenantId: tenantId,
            categoryId: categoryId,
            sku: sku,
            name: name);
        product.MarkCreated(DateTime.UtcNow, "test-user");
        return product;
    }

    /// <summary>
    /// Test implementation of ITenantContext.
    /// </summary>
    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid tenantId)
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
