namespace KromicStore.Domain.Identity;

/// <summary>
/// System role name constants. Used across authorization policies and seeding.
/// </summary>
public static class Roles
{
    public const string SuperAdmin   = "SuperAdmin";
    public const string TenantAdmin  = "TenantAdmin";
    public const string StoreManager = "StoreManager";
    public const string Customer     = "Customer";

    public static IReadOnlyList<string> All =>
    [
        SuperAdmin,
        TenantAdmin,
        StoreManager,
        Customer
    ];
}
