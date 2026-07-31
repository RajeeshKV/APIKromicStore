using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

public sealed class Tenant : AuditableEntity
{
    private readonly List<TenantDomain> _domains = [];

    private Tenant()
    {
        Name = string.Empty;
        StoreName = string.Empty;
        Slug = string.Empty;
    }

    private Tenant(Guid id, string name, string storeName, string slug) : base(id)
    {
        Name = name;
        StoreName = storeName;
        Slug = slug;
        Status = TenantStatus.Provisioning;
    }

    public string Name { get; private set; }
    public string StoreName { get; private set; }
    public string Slug { get; private set; }
    public TenantStatus Status { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    
    // Subscription & Billing
    public Guid? SubscriptionPlanId { get; private set; }
    public DateTime? SubscriptionStartDate { get; private set; }
    public DateTime? SubscriptionEndDate { get; private set; }
    public bool IsOnTrial { get; private set; }
    public DateTime? TrialEndsOn { get; private set; }
    public long StorageUsedBytes { get; private set; }
    public int EmailsSentThisMonth { get; private set; }
    public int ApiCallsUsedToday { get; private set; }
    
    // Revenue Tracking
    public decimal TotalRevenue { get; private set; }
    public decimal MonthlyRecurringRevenue { get; private set; }
    
    public IReadOnlyCollection<TenantDomain> Domains => _domains.AsReadOnly();

    public static Tenant Create(string name, string slug, string? storeName = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tenant name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Tenant slug is required.", nameof(slug));

        var normalizedSlug = NormalizeSlug(slug);
        return new Tenant(Guid.NewGuid(), name.Trim(), string.IsNullOrWhiteSpace(storeName) ? name.Trim() : storeName.Trim(), normalizedSlug);
    }

    public TenantDomain AddPlatformDomain(string subdomain, bool isPrimary)
    {
        var domain = TenantDomain.CreatePlatformDomain(Id, subdomain, isPrimary);
        _domains.Add(domain);
        return domain;
    }

    public void AssignOwner(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty) throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));
        OwnerUserId = ownerUserId;
    }

    public void RenameStore(string storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName)) throw new ArgumentException("Store name is required.", nameof(storeName));
        StoreName = storeName.Trim();
    }

    public void AssignSubscriptionPlan(Guid planId, bool isTrialPlan = false, int trialDays = 14)
    {
        if (planId == Guid.Empty) throw new ArgumentException("Plan ID is required.", nameof(planId));
        
        SubscriptionPlanId = planId;
        SubscriptionStartDate = DateTime.UtcNow;
        SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
        IsOnTrial = isTrialPlan;
        
        if (isTrialPlan)
            TrialEndsOn = DateTime.UtcNow.AddDays(trialDays);
    }

    public void UpgradeSubscriptionPlan(Guid newPlanId)
    {
        if (newPlanId == Guid.Empty) throw new ArgumentException("Plan ID is required.", nameof(newPlanId));
        SubscriptionPlanId = newPlanId;
    }

    public void CancelSubscription()
    {
        SubscriptionPlanId = null;
        SubscriptionEndDate = DateTime.UtcNow;
        IsOnTrial = false;
        TrialEndsOn = null;
    }

    public void RecordRevenue(decimal amount)
    {
        if (amount <= 0) return;
        TotalRevenue += amount;
        MonthlyRecurringRevenue += amount;
    }

    public void ResetMonthlyMetrics()
    {
        EmailsSentThisMonth = 0;
        ApiCallsUsedToday = 0;
        MonthlyRecurringRevenue = 0;
    }

    public void UpdateStorageUsage(long bytes) => StorageUsedBytes = bytes;
    public void IncrementEmailsSent() => EmailsSentThisMonth++;
    public void IncrementApiCalls() => ApiCallsUsedToday++;

    public void Activate() => Status = TenantStatus.Active;

    public void Suspend()
    {
        if (Status == TenantStatus.Archived)
        {
            throw new InvalidOperationException("Archived tenants cannot be suspended.");
        }

        Status = TenantStatus.Suspended;
    }

    public void Archive() => Status = TenantStatus.Archived;

    public void AddDomain(TenantDomain domain)
    {
        if (domain == null) throw new ArgumentNullException(nameof(domain));
        _domains.Add(domain);
    }

    public void RemoveDomain(Guid domainId)
    {
        var domain = _domains.FirstOrDefault(d => d.Id == domainId);
        if (domain != null)
        {
            _domains.Remove(domain);
        }
    }

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
}
