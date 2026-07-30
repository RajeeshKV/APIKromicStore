namespace KromicStore.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedOnUtc { get; }
    string? DeletedBy { get; }
}
