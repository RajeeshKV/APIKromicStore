namespace KromicStore.Domain.Common;

public abstract class TenantEntity : AuditableEntity, ITenantEntity
{
    protected TenantEntity()
    {
    }

    protected TenantEntity(Guid id, Guid tenantId) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required for tenant-owned entities.", nameof(tenantId));
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; private init; }
}
