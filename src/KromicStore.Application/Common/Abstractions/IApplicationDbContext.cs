using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Identity;
using KromicStore.Domain.Shopping.Entities;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Common.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<Tenant> Tenants { get; }
    IQueryable<TenantDomain> TenantDomains { get; }
    IQueryable<TenantSettings> TenantSettings { get; }
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    IQueryable<EmailVerificationToken> EmailVerificationTokens { get; }
    IQueryable<PasswordResetToken> PasswordResetTokens { get; }

    // Catalog
    IQueryable<Category> Categories { get; }
    IQueryable<Product> Products { get; }
    IQueryable<ProductCollection> ProductCollections { get; }

    // Shopping
    IQueryable<Cart> Carts { get; }
    IQueryable<CartItem> CartItems { get; }
    IQueryable<Wishlist> Wishlists { get; }
    IQueryable<WishlistItem> WishlistItems { get; }
    IQueryable<CheckoutSession> CheckoutSessions { get; }
    IQueryable<CheckoutItem> CheckoutItems { get; }

    void AddEntity<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
