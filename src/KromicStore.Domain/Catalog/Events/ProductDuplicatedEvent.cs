using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record ProductDuplicatedEvent(
    Guid SourceProductId,
    Guid TenantId,
    Guid NewProductId,
    string NewSku,
    string NewName,
    string NewSlug) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
