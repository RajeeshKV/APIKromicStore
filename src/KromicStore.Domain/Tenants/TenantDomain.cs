using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

public sealed class TenantDomain : TenantEntity
{
    private TenantDomain()
    {
    }

    private TenantDomain(Guid id, Guid tenantId, string? subdomain, string? customDomain, bool isPrimary) : base(id, tenantId)
    {
        Subdomain = subdomain;
        CustomDomain = customDomain;
        IsPrimary = isPrimary;
    }

    public string? Subdomain { get; private set; }
    public string? CustomDomain { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsVerified { get; private set; }

    public static TenantDomain CreatePlatformDomain(Guid tenantId, string subdomain, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(subdomain)) throw new ArgumentException("Subdomain is required.", nameof(subdomain));
        return new TenantDomain(Guid.NewGuid(), tenantId, NormalizeHost(subdomain), null, isPrimary);
    }

    public static TenantDomain CreateCustomDomain(Guid tenantId, string customDomain, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(customDomain)) throw new ArgumentException("Custom domain is required.", nameof(customDomain));
        return new TenantDomain(Guid.NewGuid(), tenantId, null, NormalizeHost(customDomain), isPrimary);
    }

    public void MarkVerified() => IsVerified = true;
    public void MarkPrimary() => IsPrimary = true;
    public void MarkSecondary() => IsPrimary = false;

    private static string NormalizeHost(string host) => host.Trim().TrimEnd('.').ToLowerInvariant();
}
