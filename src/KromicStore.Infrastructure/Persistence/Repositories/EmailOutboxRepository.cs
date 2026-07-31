using KromicStore.Application.Features.Email.Abstractions;
using KromicStore.Domain.Email.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Email outbox repository implementation.
/// Manages reliable email delivery with the Outbox pattern.
/// </summary>
public class EmailOutboxRepository : IEmailOutboxRepository
{
    private readonly KromicStoreDbContext _context;

    public EmailOutboxRepository(KromicStoreDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(EmailOutbox email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        await _context.Set<EmailOutbox>().AddAsync(email, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailOutbox>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EmailOutbox>> GetPendingAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailOutbox>()
            .Where(e => e.Status == EmailOutboxStatus.Pending && 
                   (e.NextRetryAtUtc == null || e.NextRetryAtUtc <= DateTime.UtcNow) &&
                   e.AttemptCount == 0)
            .OrderBy(e => e.CreatedOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailOutbox>> GetReadyForRetryAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailOutbox>()
            .Where(e => e.Status == EmailOutboxStatus.Pending && 
                   e.NextRetryAtUtc != null &&
                   e.NextRetryAtUtc <= DateTime.UtcNow &&
                   e.AttemptCount > 0)
            .OrderBy(e => e.NextRetryAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailOutbox>> GetByStatusAsync(EmailOutboxStatus status, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Set<EmailOutbox>()
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedOnUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailOutbox email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        email.MarkModified(DateTime.UtcNow, "system");
        _context.Set<EmailOutbox>().Update(email);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteOldProcessedAsync(int olderThanDays = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        return await _context.Set<EmailOutbox>()
            .Where(e => (e.Status == EmailOutboxStatus.Sent || e.Status == EmailOutboxStatus.Failed) &&
                   e.ProcessedOnUtc != null &&
                   e.ProcessedOnUtc < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
