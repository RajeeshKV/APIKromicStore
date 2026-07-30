namespace KromicStore.Application.Common.DTOs;

/// <summary>
/// Data transfer object for User entity.
/// Used in responses for user information.
/// </summary>
public sealed class UserDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
