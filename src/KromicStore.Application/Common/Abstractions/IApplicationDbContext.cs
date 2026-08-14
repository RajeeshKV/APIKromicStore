using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.CMS.Entities;
using KromicStore.Domain.Identity;
using KromicStore.Domain.Orders.Entities;
using KromicStore.Domain.Promotions.Entities;
using KromicStore.Domain.Shipping.Entities;
using KromicStore.Domain.Shopping.Entities;
using KromicStore.Domain.Taxes.Entities;
using KromicStore.Domain.Tenants;
using KromicStore.Domain.StoreOperations.Entities;
using KromicStore.Domain.CustomerPortal.Entities;
using KromicStore.Domain.Email.Entities;
using KromicStore.Domain.Media.Entities;

namespace KromicStore.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    // Tenants
    IQueryable<Tenant> Tenants { get; }
    IQueryable<TenantDomain> TenantDomains { get; }
    IQueryable<TenantSettings> TenantSettings { get; }
    IQueryable<Theme> Themes { get; }
    IQueryable<ThemeAsset> ThemeAssets { get; }
    IQueryable<SubscriptionPlan> SubscriptionPlans { get; }
    IQueryable<PlatformSettings> PlatformSettings { get; }
    IQueryable<ContactRequest> ContactRequests { get; }
    IQueryable<FeatureFlag> FeatureFlags { get; }
    IQueryable<AuditLog> AuditLogs { get; }
    
    // Identity
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    IQueryable<EmailVerificationToken> EmailVerificationTokens { get; }
    IQueryable<PasswordResetToken> PasswordResetTokens { get; }

    // Catalog
    IQueryable<Category> Categories { get; }
    IQueryable<Product> Products { get; }
    IQueryable<ProductCollection> ProductCollections { get; }

    // CMS
    IQueryable<CMSPage> CMSPages { get; }

    // Shopping
    IQueryable<Cart> Carts { get; }
    IQueryable<CartItem> CartItems { get; }
    IQueryable<Wishlist> Wishlists { get; }
    IQueryable<WishlistItem> WishlistItems { get; }
    IQueryable<CheckoutSession> CheckoutSessions { get; }
    IQueryable<CheckoutItem> CheckoutItems { get; }

    // Orders
    IQueryable<Order> Orders { get; }
    IQueryable<OrderItem> OrderItems { get; }
    IQueryable<OrderTimeline> OrderTimelines { get; }
    IQueryable<OrderNote> OrderNotes { get; }

    // Payments
    IQueryable<Payment> Payments { get; }
    IQueryable<PaymentTransaction> PaymentTransactions { get; }

    // Shipping
    IQueryable<ShippingZone> ShippingZones { get; }
    IQueryable<ShippingMethod> ShippingMethods { get; }
    IQueryable<ShippingRate> ShippingRates { get; }

    // Tax
    IQueryable<TaxRegion> TaxRegions { get; }
    IQueryable<TaxRule> TaxRules { get; }

    // Promotions
    IQueryable<Coupon> Coupons { get; }
    IQueryable<Discount> Discounts { get; }
    IQueryable<Campaign> Campaigns { get; }

    void AddEntity<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
