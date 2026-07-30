namespace KromicStore.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditable, ISoftDeletable
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id) : base(id) { }

    public DateTime CreatedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? ModifiedOnUtc { get; private set; }
    public string? ModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    public void MarkCreated(DateTime utcNow, string actor)
    {
        if (CreatedOnUtc != default) return;
        CreatedOnUtc = EnsureUtc(utcNow);
        CreatedBy = NormalizeActor(actor);
    }

    public void MarkModified(DateTime utcNow, string actor)
    {
        ModifiedOnUtc = EnsureUtc(utcNow);
        ModifiedBy = NormalizeActor(actor);
    }

    public void SoftDelete(DateTime utcNow, string actor)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedOnUtc = EnsureUtc(utcNow);
        DeletedBy = NormalizeActor(actor);
        MarkModified(utcNow, actor);
    }

    public void Restore(DateTime utcNow, string actor)
    {
        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedBy = null;
        MarkModified(utcNow, actor);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string NormalizeActor(string actor) =>
        string.IsNullOrWhiteSpace(actor) ? "System" : actor.Trim();
}
