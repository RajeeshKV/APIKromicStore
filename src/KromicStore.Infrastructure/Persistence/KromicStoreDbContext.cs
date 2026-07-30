using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Common;
using KromicStore.Domain.Identity;
using KromicStore.Domain.Shopping.Entities;
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
    }
}



