namespace KromicStore.Domain.Tenants;

public static class TenantStatusExtensions
{
    public static bool IsActive(this TenantStatus status) => status == TenantStatus.Active;
    
    public static bool IsInactive(this TenantStatus status) => status is TenantStatus.Suspended or TenantStatus.Archived or TenantStatus.Provisioning;
}
