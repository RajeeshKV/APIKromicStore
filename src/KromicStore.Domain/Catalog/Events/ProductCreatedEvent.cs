using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    Guid TenantId,
    Guid CategoryId,
    string Sku,
    string Name) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
