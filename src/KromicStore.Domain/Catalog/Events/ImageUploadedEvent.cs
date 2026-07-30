using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Events;

public sealed record ImageUploadedEvent(
    Guid ProductId,
    Guid TenantId,
    Guid ImageId,
    string Url) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
