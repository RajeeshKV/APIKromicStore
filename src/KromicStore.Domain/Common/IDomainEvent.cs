namespace KromicStore.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
