using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record ProductUpdatedEvent(
    Guid ProductId,
    Guid TenantId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
