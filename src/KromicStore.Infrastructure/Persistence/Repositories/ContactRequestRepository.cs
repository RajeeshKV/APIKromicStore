using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Contact request repository for customer support and inquiries.
/// Manages customer contact form submissions and support ticket lifecycle.
/// </summary>
public sealed class ContactRequestRepository : IContactRequestRepository
{
    private readonly IApplicationDbContext _dbContext;

    public ContactRequestRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ContactRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactRequests
            .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
    }

    public async Task<List<ContactRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactRequests
            .OrderByDescending(cr => cr.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ContactRequest>> GetByStatusAsync(
        ContactRequestStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactRequests
            .Where(cr => cr.Status == status)
            .OrderByDescending(cr => cr.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ContactRequest> Requests, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        ContactRequestStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ContactRequests.AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(cr => cr.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLowerInvariant();
            query = query.Where(cr => cr.Email.ToLower().Contains(lowerSearch) ||
                                     cr.Subject.ToLower().Contains(lowerSearch) ||
                                     cr.Message.ToLower().Contains(lowerSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(cr => cr.CreatedOnUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (requests, totalCount);
    }

    public async Task<int> GetUnresolvedCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContactRequests
            .Where(cr => cr.Status != ContactRequestStatus.Resolved && cr.Status != ContactRequestStatus.Archived)
            .CountAsync(cancellationToken);
    }

    public void Add(ContactRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        _dbContext.AddEntity(request);
    }

    public void Update(ContactRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        // Update is handled by EF Core tracking
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
