namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Service for checking reserved subdomains.
/// Prevents tenant creation with platform-reserved names.
/// </summary>
public interface IReservedSubdomainService
{
    /// <summary>
    /// Determines if a subdomain is reserved and cannot be used.
    /// </summary>
    bool IsReserved(string subdomain);
}
