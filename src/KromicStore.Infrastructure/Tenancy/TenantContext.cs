using KromicStore.Application.Common.Abstractions;

namespace KromicStore.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public Guid? StoreId { get; private set; }
    public string? StoreName { get; private set; }
    public string? Locale { get; private set; }
    public string? TimeZone { get; private set; }
    public bool IsResolved => TenantId.HasValue;

    public void Set(Guid tenantId, Guid? storeId = null, string? storeName = null, string? locale = null, string? timeZone = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        TenantId = tenantId;
        StoreId = storeId;
        StoreName = storeName;
        Locale = locale;
        TimeZone = timeZone;
    }
}
