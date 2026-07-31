using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Payment aggregate root.
/// Enforces tenant isolation and provides payment data access operations.
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public PaymentRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            return null;

        return await _dbContext.Payments
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        return await _dbContext.Payments
            .Where(p => p.CustomerId == customerId)
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(p => p.Status == status)
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetRetryScheduledPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Payments
            .Where(p => p.Status == PaymentStatus.RetryScheduled && 
                       p.NextRetryAtUtc.HasValue && 
                       p.NextRetryAtUtc <= now)
            .Include(p => p.Transactions)
            .OrderBy(p => p.NextRetryAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> PaymentExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            return false;

        return await _dbContext.Payments
            .AnyAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<Payment?> GetByProviderTransactionIdAsync(string providerTransactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerTransactionId))
            return null;

        return await _dbContext.Payments
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.ProviderTransactionId == providerTransactionId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetFailedPaymentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        return await _dbContext.Payments
            .Where(p => p.CustomerId == customerId && 
                       (p.Status == PaymentStatus.Failed || p.Status == PaymentStatus.RetryScheduled))
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _dbContext.PaymentSet.Add(payment);
    }

    public void Update(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _dbContext.PaymentSet.Update(payment);
    }

    public void Remove(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _dbContext.PaymentSet.Remove(payment);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
