using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using KromicStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Shopping.Common;

/// <summary>
/// Common test fixtures and utilities for Shopping tests.
/// </summary>
public static class ShoppingTestFixtures
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
    /// Creates a test cart for a customer with default values.
    /// </summary>
    public static Cart CreateTestCustomerCart(
        Guid tenantId,
        Guid customerId,
        string currency = "USD",
        Guid? cartId = null)
    {
        // Create cart with the provided or random ID via reflection
        var cart = CreateCartWithId(
            () => Cart.CreateForCustomer(tenantId, customerId, currency),
            cartId);
        return cart;
    }

    /// <summary>
    /// Creates a test cart for a guest with default values.
    /// </summary>
    public static Cart CreateTestGuestCart(
        Guid tenantId,
        string sessionId = "guest-session-123",
        string currency = "USD",
        Guid? cartId = null)
    {
        // Create cart with the provided or random ID via reflection
        var cart = CreateCartWithId(
            () => Cart.CreateForGuest(tenantId, sessionId, currency),
            cartId);
        return cart;
    }

    /// <summary>
    /// Helper method to create a cart with a specific ID for testing.
    /// Uses reflection to set the ID after object creation since Id has a private init accessor.
    /// </summary>
    private static Cart CreateCartWithId(Func<Cart> factory, Guid? cartId)
    {
        var cart = factory();
        
        if (cartId.HasValue && cart.Id != cartId.Value)
        {
            // Need to set the backing field for the Id property
            // The compiler generates a backing field like <Id>k__BackingField for init-only properties
            var type = cart.GetType();
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            // Try the compiler-generated backing field first
            var idField = type.GetField("<Id>k__BackingField", bindingFlags);
            
            // If not found, try walking up the type hierarchy
            if (idField == null)
            {
                var baseType = type.BaseType;
                while (baseType != null && idField == null)
                {
                    idField = baseType.GetField("<Id>k__BackingField", bindingFlags);
                    baseType = baseType.BaseType;
                }
            }
            
            if (idField != null)
            {
                idField.SetValue(cart, cartId.Value);
            }
        }
        
        return cart;
    }

    /// <summary>
    /// Creates a test checkout session with optional custom ID for testing.
    /// </summary>
    public static CheckoutSession CreateTestCheckoutSession(
        Guid tenantId,
        Guid customerId,
        Guid? checkoutSessionId = null)
    {
        var session = CheckoutSession.Create(tenantId, customerId);
        
        if (checkoutSessionId.HasValue && session.Id != checkoutSessionId.Value)
        {
            // Set the backing field for the Id property
            var type = session.GetType();
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            var idField = type.GetField("<Id>k__BackingField", bindingFlags);
            if (idField == null)
            {
                var baseType = type.BaseType;
                while (baseType != null && idField == null)
                {
                    idField = baseType.GetField("<Id>k__BackingField", bindingFlags);
                    baseType = baseType.BaseType;
                }
            }
            
            if (idField != null)
            {
                idField.SetValue(session, checkoutSessionId.Value);
            }
        }
        
        return session;
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
