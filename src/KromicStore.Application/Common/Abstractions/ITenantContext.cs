namespace KromicStore.Application.Common.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? StoreId { get; }
    string? StoreName { get; }
    string? Locale { get; }
    string? TimeZone { get; }
    bool IsResolved { get; }
}
