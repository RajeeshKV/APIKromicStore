using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Represents a subscription plan offered by the platform.
/// Defines pricing tiers, feature limits, and constraints for tenants.
/// </summary>
public sealed class SubscriptionPlan : AuditableEntity
{
    private SubscriptionPlan()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    private SubscriptionPlan(Guid id, string name, string description, decimal monthlyPrice, bool isActive)
        : base(id)
    {
        Name = name;
        Description = description;
        MonthlyPrice = monthlyPrice;
        IsActive = isActive;
    }

    // Plan Identification
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int DisplayOrder { get; private set; }

    // Pricing
    public decimal MonthlyPrice { get; private set; }
    public decimal? AnnualPrice { get; private set; }
    public decimal? TrialPricePerDay { get; private set; }
    public int TrialDays { get; private set; } = 14;

    // Feature Limits
    public int MaxProducts { get; private set; } = 100;
    public int MaxCategories { get; private set; } = 20;
    public int MaxCollections { get; private set; } = 10;
    public int MaxStaff { get; private set; } = 5;
    public int MaxCustomers { get; private set; } = 999_999;

    // Storage & Resources
    public long MaxStorageBytes { get; private set; } = 5_368_709_120; // 5 GB
    public int MaxEmailsPerMonth { get; private set; } = 10000;
    public int MaxApiCallsPerDay { get; private set; } = 100000;

    // Capabilities
    public bool CanCustomizeDomain { get; private set; }
    public bool CanUseThemes { get; private set; } = true;
    public bool CanUseCustomTheme { get; private set; }
    public bool CanUsePaymentGateway { get; private set; } = true;
    public bool CanUseAdvancedReporting { get; private set; }
    public bool CanUseAnalytics { get; private set; } = true;
    public bool CanUseEmailMarketing { get; private set; }
    public bool CanUseSeo { get; private set; }
    public bool CanUseMultipleCurrencies { get; private set; }
    public bool CanUseAdvancedInventory { get; private set; }
    public bool CanUseWebhooks { get; private set; }
    public bool CanUsePrioritySupportEmail { get; private set; }
    public bool CanUsePrioritySupportPhone { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public bool IsTrial { get; private set; }

    public static SubscriptionPlan Create(
        string name,
        string description,
        decimal monthlyPrice,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.", nameof(name));

        return new SubscriptionPlan(Guid.NewGuid(), name.Trim(), description.Trim(), monthlyPrice, isActive);
    }

    public void Update(
        string name,
        string description,
        decimal monthlyPrice,
        decimal? annualPrice = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.", nameof(name));

        Name = name.Trim();
        Description = description.Trim();
        MonthlyPrice = monthlyPrice;
        AnnualPrice = annualPrice;
        DisplayOrder = displayOrder;
    }

    public void SetFeatureLimits(
        int maxProducts,
        int maxCategories,
        int maxCollections,
        int maxStaff,
        int maxCustomers)
    {
        if (maxProducts <= 0) throw new ArgumentException("Max products must be positive.", nameof(maxProducts));
        if (maxCategories <= 0) throw new ArgumentException("Max categories must be positive.", nameof(maxCategories));
        if (maxStaff <= 0) throw new ArgumentException("Max staff must be positive.", nameof(maxStaff));

        MaxProducts = maxProducts;
        MaxCategories = maxCategories;
        MaxCollections = maxCollections;
        MaxStaff = maxStaff;
        MaxCustomers = maxCustomers;
    }

    public void SetStorageLimits(long maxStorageBytes, int maxEmailsPerMonth, int maxApiCallsPerDay)
    {
        if (maxStorageBytes <= 0) throw new ArgumentException("Max storage must be positive.", nameof(maxStorageBytes));
        if (maxEmailsPerMonth <= 0) throw new ArgumentException("Max emails must be positive.", nameof(maxEmailsPerMonth));
        if (maxApiCallsPerDay <= 0) throw new ArgumentException("Max API calls must be positive.", nameof(maxApiCallsPerDay));

        MaxStorageBytes = maxStorageBytes;
        MaxEmailsPerMonth = maxEmailsPerMonth;
        MaxApiCallsPerDay = maxApiCallsPerDay;
    }

    public void SetCapabilities(
        bool canCustomizeDomain = false,
        bool canUseCustomTheme = false,
        bool canUseAdvancedReporting = false,
        bool canUseEmailMarketing = false,
        bool canUseSeo = false,
        bool canUseMultipleCurrencies = false,
        bool canUseAdvancedInventory = false,
        bool canUseWebhooks = false,
        bool canUsePrioritySupportEmail = false,
        bool canUsePrioritySupportPhone = false)
    {
        CanCustomizeDomain = canCustomizeDomain;
        CanUseCustomTheme = canUseCustomTheme;
        CanUseAdvancedReporting = canUseAdvancedReporting;
        CanUseEmailMarketing = canUseEmailMarketing;
        CanUseSeo = canUseSeo;
        CanUseMultipleCurrencies = canUseMultipleCurrencies;
        CanUseAdvancedInventory = canUseAdvancedInventory;
        CanUseWebhooks = canUseWebhooks;
        CanUsePrioritySupportEmail = canUsePrioritySupportEmail;
        CanUsePrioritySupportPhone = canUsePrioritySupportPhone;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void MarkAsTrial() => IsTrial = true;
}
