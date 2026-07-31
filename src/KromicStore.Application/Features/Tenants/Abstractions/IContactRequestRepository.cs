using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for ContactRequest aggregate.
/// </summary>
public interface IContactRequestRepository
{
    Task<ContactRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ContactRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ContactRequest>> GetByStatusAsync(
        ContactRequestStatus status,
        CancellationToken cancellationToken = default);
    Task<(List<ContactRequest> Requests, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        ContactRequestStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<int> GetUnresolvedCountAsync(CancellationToken cancellationToken = default);
    void Add(ContactRequest request);
    void Update(ContactRequest request);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
