using MediatR;

namespace KromicStore.Application.CustomerPortal.Queries.GetProfile;

/// <summary>
/// Query to retrieve customer profile information.
/// </summary>
public sealed class GetProfileQuery : IRequest<GetProfileResponse>
{
    public Guid CustomerId { get; set; }
}

public sealed class GetProfileResponse
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public bool NewsletterOptIn { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public int LoginCount { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
