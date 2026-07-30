namespace KromicStore.Application.Common.DTOs;

/// <summary>
/// Data transfer object for Tenant entity.
/// Used in responses for tenant information.
/// </summary>
public sealed class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? OwnerUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
