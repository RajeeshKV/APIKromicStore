namespace KromicStore.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected BaseEntity()
    {
    }

    protected BaseEntity(Guid id)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
    }

    public Guid Id { get; private init; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public bool Equals(BaseEntity? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is BaseEntity entity && Equals(entity);
    public override int GetHashCode() => Id.GetHashCode();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
