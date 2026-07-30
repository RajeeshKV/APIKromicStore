namespace KromicStore.Domain.Common;

public interface IAuditable
{
    DateTime CreatedOnUtc { get; }
    string CreatedBy { get; }
    DateTime? ModifiedOnUtc { get; }
    string? ModifiedBy { get; }
}
