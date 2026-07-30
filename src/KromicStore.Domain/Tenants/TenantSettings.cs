using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

public sealed class TenantSettings : TenantEntity
{
    private TenantSettings()
    {
        Currency = string.Empty;
        TimeZone = string.Empty;
        Language = string.Empty;
        OrderPrefix = string.Empty;
    }

    private TenantSettings(Guid tenantId) : base(Guid.NewGuid(), tenantId)
    {
        Currency = "INR";
        TimeZone = "Asia/Kolkata";
        Language = "en";
        OrderPrefix = "ORD";
        AllowGuestCheckout = true;
        EnableWishlist = true;
        EnableReviews = false;
    }

    public string Currency { get; private set; }
    public string TimeZone { get; private set; }
    public string Language { get; private set; }
    public string OrderPrefix { get; private set; }
    public bool AllowGuestCheckout { get; private set; }
    public bool EnableWishlist { get; private set; }
    public bool EnableReviews { get; private set; }
    public bool MaintenanceMode { get; private set; }

    // Branding
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }

    // Contact Information
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    // Address
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }

    // Payment Integration
    public string? RazorpayKeyId { get; private set; }
    public string? RazorpayKeySecret { get; private set; }

    public static TenantSettings CreateDefault(Guid tenantId) => new(tenantId);

    public void UpdateBranding(string? logoUrl, string? faviconUrl, string? primaryColor, string? secondaryColor)
    {
        LogoUrl = logoUrl;
        FaviconUrl = faviconUrl;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public void UpdateContactInfo(string? email, string? phone)
    {
        ContactEmail = email;
        ContactPhone = phone;
    }

    public void UpdateAddress(string? address, string? city, string? state, string? country, string? postalCode)
    {
        Address = address;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public void UpdateRazorpayCredentials(string? keyId, string? keySecret)
    {
        RazorpayKeyId = keyId;
        RazorpayKeySecret = keySecret;
    }
}
