using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record InventoryAdjustedEvent(
    Guid ProductId,
    Guid? VariantId,
    Guid TenantId,
    int QuantityChange,
    int NewAvailableQuantity,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
