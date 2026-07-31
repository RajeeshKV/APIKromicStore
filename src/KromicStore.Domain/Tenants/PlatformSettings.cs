using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Platform-wide settings managed by Super Users.
/// Singleton entity - only one instance per platform.
/// </summary>
public sealed class PlatformSettings : AuditableEntity
{
    // Use a fixed ID for singleton pattern
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private PlatformSettings()
    {
        PlatformName = string.Empty;
        SupportEmail = string.Empty;
        DefaultCurrency = "USD";
        DefaultTimezone = "UTC";
    }

    // General Settings
    public string PlatformName { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }

    // Contact Settings
    public string SupportEmail { get; private set; }
    public string? SupportPhoneNumber { get; private set; }
    public string? ContactFormEmail { get; private set; }

    // Platform URLs
    public string? LandingPageUrl { get; private set; }
    public string? FooterContent { get; private set; }
    public string? PrivacyPolicyUrl { get; private set; }
    public string? TermsOfServiceUrl { get; private set; }

    // Defaults
    public string DefaultCurrency { get; private set; }
    public string DefaultTimezone { get; private set; }

    // SMTP Configuration
    public string? SmtpHost { get; private set; }
    public int? SmtpPort { get; private set; }
    public string? SmtpUsername { get; private set; }
    public string? SmtpPassword { get; private set; }
    public bool SmtpUseSsl { get; private set; }
    public string? SmtpFromAddress { get; private set; }
    public string? SmtpFromName { get; private set; }

    // Email Templates
    public string? WelcomeEmailTemplate { get; private set; }
    public string? ResetPasswordEmailTemplate { get; private set; }
    public string? MaintenanceNoticeEmailTemplate { get; private set; }

    // Storage Configuration
    public string? CloudinaryCloudName { get; private set; }
    public string? CloudinaryApiKey { get; private set; }
    // NOTE: API Secret should NOT be stored in settings; use secure vault instead

    // Payment Gateway Configuration
    public string? RazorpayKeyId { get; private set; }
    public bool RazorpayEnabled { get; private set; }
    public bool? StripeEnabled { get; private set; }
    public bool? PayPalEnabled { get; private set; }
    // NOTE: Key Secrets should NOT be stored in settings; use secure vault instead

    // Feature Flags
    public bool MaintenanceMode { get; private set; }
    public string? MaintenanceMessage { get; private set; }
    public bool AllowNewTenantSignups { get; private set; } = true;
    public bool AllowTrialSignups { get; private set; } = true;
    public bool RequireEmailVerification { get; private set; } = true;
    public bool RequireManualApproval { get; private set; }

    // Limits
    public int MaxTenantsPerPlatform { get; private set; } = 999999;
    public int MaxFreeTrialDays { get; private set; } = 14;
    public decimal MinimumMonthlyPrice { get; private set; } = 0m;

    // Analytics & Monitoring
    public bool EnableAnalytics { get; private set; } = true;
    public bool EnablePerformanceMonitoring { get; private set; } = true;
    public bool EnableErrorTracking { get; private set; } = true;
    public int AnalyticsRetentionDays { get; private set; } = 90;

    public static PlatformSettings Create(
        string platformName,
        string supportEmail)
    {
        if (string.IsNullOrWhiteSpace(platformName))
            throw new ArgumentException("Platform name is required.", nameof(platformName));
        if (string.IsNullOrWhiteSpace(supportEmail))
            throw new ArgumentException("Support email is required.", nameof(supportEmail));

        // Create new instance - BaseEntity constructor handles ID
        var settings = new PlatformSettings
        {
            PlatformName = platformName.Trim(),
            SupportEmail = supportEmail.Trim()
        };

        return settings;
    }

    public void UpdateGeneralSettings(
        string platformName,
        string? logoUrl,
        string? faviconUrl)
    {
        if (string.IsNullOrWhiteSpace(platformName))
            throw new ArgumentException("Platform name is required.", nameof(platformName));

        PlatformName = platformName.Trim();
        LogoUrl = logoUrl;
        FaviconUrl = faviconUrl;
    }

    public void UpdateContactSettings(
        string supportEmail,
        string? supportPhoneNumber,
        string? contactFormEmail)
    {
        if (string.IsNullOrWhiteSpace(supportEmail))
            throw new ArgumentException("Support email is required.", nameof(supportEmail));

        SupportEmail = supportEmail.Trim();
        SupportPhoneNumber = supportPhoneNumber;
        ContactFormEmail = contactFormEmail;
    }

    public void UpdateDefaultSettings(
        string defaultCurrency,
        string defaultTimezone)
    {
        if (string.IsNullOrWhiteSpace(defaultCurrency))
            throw new ArgumentException("Default currency is required.", nameof(defaultCurrency));
        if (string.IsNullOrWhiteSpace(defaultTimezone))
            throw new ArgumentException("Default timezone is required.", nameof(defaultTimezone));

        DefaultCurrency = defaultCurrency.Trim().ToUpperInvariant();
        DefaultTimezone = defaultTimezone.Trim();
    }

    public void UpdateSmtpSettings(
        string host,
        int port,
        string username,
        string password,
        bool useSsl,
        string fromAddress,
        string fromName)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("SMTP host is required.", nameof(host));

        SmtpHost = host.Trim();
        SmtpPort = port;
        SmtpUsername = username;
        SmtpPassword = password; // Should be encrypted in practice
        SmtpUseSsl = useSsl;
        SmtpFromAddress = fromAddress;
        SmtpFromName = fromName;
    }

    public void UpdateStorageConfiguration(
        string cloudinaryCloudName,
        string cloudinaryApiKey)
    {
        if (string.IsNullOrWhiteSpace(cloudinaryCloudName))
            throw new ArgumentException("Cloudinary cloud name is required.", nameof(cloudinaryCloudName));

        CloudinaryCloudName = cloudinaryCloudName.Trim();
        CloudinaryApiKey = cloudinaryApiKey;
    }

    public void UpdatePaymentDefaults(
        string razorpayKeyId,
        bool razorpayEnabled = true,
        bool? stripeEnabled = null,
        bool? paypalEnabled = null)
    {
        if (string.IsNullOrWhiteSpace(razorpayKeyId))
            throw new ArgumentException("Razorpay key ID is required.", nameof(razorpayKeyId));

        RazorpayKeyId = razorpayKeyId.Trim();
        RazorpayEnabled = razorpayEnabled;
        StripeEnabled = stripeEnabled;
        PayPalEnabled = paypalEnabled;
    }

    public void SetMaintenanceMode(bool enabled, string? message = null)
    {
        MaintenanceMode = enabled;
        MaintenanceMessage = message;
    }

    public void UpdateSignupSettings(
        bool allowNewTenantSignups,
        bool allowTrialSignups,
        bool requireEmailVerification,
        bool requireManualApproval)
    {
        AllowNewTenantSignups = allowNewTenantSignups;
        AllowTrialSignups = allowTrialSignups;
        RequireEmailVerification = requireEmailVerification;
        RequireManualApproval = requireManualApproval;
    }
}
