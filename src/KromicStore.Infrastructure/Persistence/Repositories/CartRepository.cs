using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Cart aggregate root.
/// Handles persistence operations with tenant isolation.
/// </summary>
public sealed class CartRepository : ICartRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CartRepository(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        if (cartId == Guid.Empty)
            return null;

        return await _dbContext.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return null;

        return await _dbContext.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.CustomerId == customerId &&
                !c.IsDeleted &&
                DateTime.UtcNow <= c.ExpiresOnUtc,
                cancellationToken);
    }

    public async Task<Cart?> GetByAnonymousSessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        return await _dbContext.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.AnonymousSessionId == sessionId &&
                !c.IsDeleted &&
                DateTime.UtcNow <= c.ExpiresOnUtc,
                cancellationToken);
    }

    public async Task<bool> HasActiveCartAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return false;

        return await _dbContext.Carts
            .AnyAsync(c =>
                c.CustomerId == customerId &&
                !c.IsDeleted &&
                DateTime.UtcNow <= c.ExpiresOnUtc,
                cancellationToken);
    }

    public void Add(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        _dbContext.AddEntity(cart);
    }

    public void Update(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        // EF Core tracks changes automatically, but we can be explicit
        _dbContext.AddEntity(cart);
    }

    public void Remove(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        // Soft delete is handled by the DbContext SaveChangesAsync
        _dbContext.AddEntity(cart);
    }

    public async Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Carts
            .AsNoTracking()
            .Where(c =>
                !c.IsDeleted &&
                DateTime.UtcNow > c.ExpiresOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
