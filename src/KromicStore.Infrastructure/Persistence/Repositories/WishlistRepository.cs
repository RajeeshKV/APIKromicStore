using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Wishlist aggregate root.
/// Handles persistence operations with tenant isolation.
/// </summary>
public sealed class WishlistRepository : IWishlistRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public WishlistRepository(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Wishlist?> GetByIdAsync(Guid wishlistId, CancellationToken cancellationToken = default)
    {
        if (wishlistId == Guid.Empty)
            return null;

        return await _dbContext.Wishlists
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == wishlistId, cancellationToken);
    }

    public async Task<Wishlist?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return null;

        return await _dbContext.Wishlists
            .AsNoTracking()
            .FirstOrDefaultAsync(w =>
                w.CustomerId == customerId &&
                !w.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> ExistsForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return false;

        return await _dbContext.Wishlists
            .AnyAsync(w =>
                w.CustomerId == customerId &&
                !w.IsDeleted,
                cancellationToken);
    }

    public void Add(Wishlist wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        _dbContext.AddEntity(wishlist);
    }

    public void Update(Wishlist wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        // EF Core tracks changes automatically, but we can be explicit
        _dbContext.AddEntity(wishlist);
    }

    public void Remove(Wishlist wishlist)
    {
        if (wishlist == null)
            throw new ArgumentNullException(nameof(wishlist));

        // Soft delete is handled by the DbContext SaveChangesAsync
        _dbContext.AddEntity(wishlist);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
