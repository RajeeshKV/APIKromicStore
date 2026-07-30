using KromicStore.Domain.Tenants;

namespace KromicStore.Domain.Tests;

public sealed class TenantTests
{
    [Fact]
    public void Create_ShouldNormalizeSlug()
    {
        var tenant = Tenant.Create("Kromic Demo", " DEMO ");

        Assert.Equal("demo", tenant.Slug);
        Assert.Equal(TenantStatus.Provisioning, tenant.Status);
    }

    [Fact]
    public void Suspend_ShouldRejectArchivedTenant()
    {
        var tenant = Tenant.Create("Kromic Demo", "demo");
        tenant.Archive();

        Assert.Throws<InvalidOperationException>(tenant.Suspend);
    }
}
