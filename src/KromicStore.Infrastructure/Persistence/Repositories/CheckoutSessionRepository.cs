using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for CheckoutSession aggregate root.
/// Handles persistence operations with tenant isolation and checkout state management.
/// </summary>
public sealed class CheckoutSessionRepository : ICheckoutSessionRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CheckoutSessionRepository(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CheckoutSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty)
            return null;

        return await _dbContext.CheckoutSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Id == sessionId, cancellationToken);
    }

    public async Task<CheckoutSession?> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return null;

        return await _dbContext.CheckoutSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(cs =>
                cs.CustomerId == customerId &&
                cs.Status == CheckoutSessionStatus.Draft &&
                (!cs.ExpiresOnUtc.HasValue || DateTime.UtcNow <= cs.ExpiresOnUtc.Value) &&
                !cs.IsDeleted,
                cancellationToken);
    }

    public async Task<IEnumerable<CheckoutSession>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        return await _dbContext.CheckoutSessions
            .AsNoTracking()
            .Where(cs =>
                cs.CustomerId == customerId &&
                !cs.IsDeleted)
            .OrderByDescending(cs => cs.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveCheckoutAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return false;

        return await _dbContext.CheckoutSessions
            .AnyAsync(cs =>
                cs.CustomerId == customerId &&
                cs.Status == CheckoutSessionStatus.Draft &&
                (!cs.ExpiresOnUtc.HasValue || DateTime.UtcNow <= cs.ExpiresOnUtc.Value) &&
                !cs.IsDeleted,
                cancellationToken);
    }

    public void Add(CheckoutSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _dbContext.AddEntity(session);
    }

    public void Update(CheckoutSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // EF Core tracks changes automatically, but we can be explicit
        _dbContext.AddEntity(session);
    }

    public void Remove(CheckoutSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // Soft delete is handled by the DbContext SaveChangesAsync
        _dbContext.AddEntity(session);
    }

    public async Task<IEnumerable<CheckoutSession>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CheckoutSessions
            .AsNoTracking()
            .Where(cs =>
                !cs.IsDeleted &&
                cs.ExpiresOnUtc.HasValue &&
                DateTime.UtcNow > cs.ExpiresOnUtc.Value &&
                cs.Status != CheckoutSessionStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
