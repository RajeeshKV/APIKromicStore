using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record VariantCreatedEvent(
    Guid ProductId,
    Guid TenantId,
    Guid VariantId,
    string Sku) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
