using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Common;
using KromicStore.Domain.Identity;
using KromicStore.Domain.Orders.Entities;
using KromicStore.Domain.Promotions.Entities;
using KromicStore.Domain.Shipping.Entities;
using KromicStore.Domain.Shopping.Entities;
using KromicStore.Domain.Taxes.Entities;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence;

public sealed class KromicStoreDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService? _currentUserService;

    public KromicStoreDbContext(
        DbContextOptions<KromicStoreDbContext> options,
        ITenantContext tenantContext,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public DbSet<Tenant> TenantSet => Set<Tenant>();
    public IQueryable<Tenant> Tenants => TenantSet;

    public DbSet<TenantDomain> TenantDomainSet => Set<TenantDomain>();
    public IQueryable<TenantDomain> TenantDomains => TenantDomainSet;

    public DbSet<TenantSettings> TenantSettingsSet => Set<TenantSettings>();
    public IQueryable<TenantSettings> TenantSettings => TenantSettingsSet;

    public DbSet<User> UserSet => Set<User>();
    public IQueryable<User> Users => UserSet;

    public DbSet<Role> RoleSet => Set<Role>();
    public IQueryable<Role> Roles => RoleSet;

    public DbSet<RefreshToken> RefreshTokenSet => Set<RefreshToken>();
    public IQueryable<RefreshToken> RefreshTokens => RefreshTokenSet;

    public DbSet<EmailVerificationToken> EmailVerificationTokenSet => Set<EmailVerificationToken>();
    public IQueryable<EmailVerificationToken> EmailVerificationTokens => EmailVerificationTokenSet;

    public DbSet<PasswordResetToken> PasswordResetTokenSet => Set<PasswordResetToken>();
    public IQueryable<PasswordResetToken> PasswordResetTokens => PasswordResetTokenSet;

    // Catalog DbSets
    public DbSet<Category> CategorySet => Set<Category>();
    public IQueryable<Category> Categories => CategorySet;

    public DbSet<Product> ProductSet => Set<Product>();
    public IQueryable<Product> Products => ProductSet;

    public DbSet<ProductCollection> ProductCollectionSet => Set<ProductCollection>();
    public IQueryable<ProductCollection> ProductCollections => ProductCollectionSet;

    // Shopping DbSets (Cart, Wishlist, Checkout)
    public DbSet<Cart> CartSet => Set<Cart>();
    public IQueryable<Cart> Carts => CartSet;

    public DbSet<CartItem> CartItemSet => Set<CartItem>();
    public IQueryable<CartItem> CartItems => CartItemSet;

    public DbSet<Wishlist> WishlistSet => Set<Wishlist>();
    public IQueryable<Wishlist> Wishlists => WishlistSet;

    public DbSet<WishlistItem> WishlistItemSet => Set<WishlistItem>();
    public IQueryable<WishlistItem> WishlistItems => WishlistItemSet;

    public DbSet<CheckoutSession> CheckoutSessionSet => Set<CheckoutSession>();
    public IQueryable<CheckoutSession> CheckoutSessions => CheckoutSessionSet;

    public DbSet<CheckoutItem> CheckoutItemSet => Set<CheckoutItem>();
    public IQueryable<CheckoutItem> CheckoutItems => CheckoutItemSet;

    // Orders DbSets
    public DbSet<Order> OrderSet => Set<Order>();
    public IQueryable<Order> Orders => OrderSet;

    public DbSet<OrderItem> OrderItemSet => Set<OrderItem>();
    public IQueryable<OrderItem> OrderItems => OrderItemSet;

    public DbSet<OrderTimeline> OrderTimelineSet => Set<OrderTimeline>();
    public IQueryable<OrderTimeline> OrderTimelines => OrderTimelineSet;

    public DbSet<OrderNote> OrderNoteSet => Set<OrderNote>();
    public IQueryable<OrderNote> OrderNotes => OrderNoteSet;

    // Payments DbSets
    public DbSet<Payment> PaymentSet => Set<Payment>();
    public IQueryable<Payment> Payments => PaymentSet;

    public DbSet<PaymentTransaction> PaymentTransactionSet => Set<PaymentTransaction>();
    public IQueryable<PaymentTransaction> PaymentTransactions => PaymentTransactionSet;

    // Shipping DbSets
    public DbSet<ShippingZone> ShippingZoneSet => Set<ShippingZone>();
    public IQueryable<ShippingZone> ShippingZones => ShippingZoneSet;

    public DbSet<ShippingMethod> ShippingMethodSet => Set<ShippingMethod>();
    public IQueryable<ShippingMethod> ShippingMethods => ShippingMethodSet;

    public DbSet<ShippingRate> ShippingRateSet => Set<ShippingRate>();
    public IQueryable<ShippingRate> ShippingRates => ShippingRateSet;

    // Tax DbSets
    public DbSet<TaxRegion> TaxRegionSet => Set<TaxRegion>();
    public IQueryable<TaxRegion> TaxRegions => TaxRegionSet;

    public DbSet<TaxRule> TaxRuleSet => Set<TaxRule>();
    public IQueryable<TaxRule> TaxRules => TaxRuleSet;

    // Promotions DbSets
    public DbSet<Coupon> CouponSet => Set<Coupon>();
    public IQueryable<Coupon> Coupons => CouponSet;

    public DbSet<Discount> DiscountSet => Set<Discount>();
    public IQueryable<Discount> Discounts => DiscountSet;

    public DbSet<Campaign> CampaignSet => Set<Campaign>();
    public IQueryable<Campaign> Campaigns => CampaignSet;

    public void AddEntity<T>(T entity) where T : class => Set<T>().Add(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KromicStoreDbContext).Assembly);
        ApplyTenantAndSoftDeleteFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditRules()
    {
        var utcNow = DateTime.UtcNow;
        var actor = _currentUserService?.IsAuthenticated == true ? _currentUserService.UserId.ToString() : "System";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkCreated(utcNow, actor);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkModified(utcNow, actor);
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(utcNow, actor);
            }
        }
    }

    private void ApplyTenantAndSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>().HasQueryFilter(entity => !entity.IsDeleted);
        modelBuilder.Entity<TenantDomain>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<TenantSettings>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<User>().HasQueryFilter(entity => !entity.IsDeleted && (!entity.TenantId.HasValue || (_tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId)));
        modelBuilder.Entity<Role>().HasQueryFilter(entity => !entity.IsDeleted);

        // Catalog query filters
        modelBuilder.Entity<Category>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ProductCollection>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);

        // Shopping query filters
        modelBuilder.Entity<Cart>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Wishlist>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<CheckoutSession>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);

        // Orders query filters
        modelBuilder.Entity<Order>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        
        // Payments query filters
        modelBuilder.Entity<Payment>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);

        // Shipping query filters
        modelBuilder.Entity<ShippingZone>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);

        // Tax query filters
        modelBuilder.Entity<TaxRegion>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);

        // Promotions query filters
        modelBuilder.Entity<Coupon>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Discount>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Campaign>().HasQueryFilter(entity => !entity.IsDeleted && _tenantContext.TenantId.HasValue && entity.TenantId == _tenantContext.TenantId);
    }
}



