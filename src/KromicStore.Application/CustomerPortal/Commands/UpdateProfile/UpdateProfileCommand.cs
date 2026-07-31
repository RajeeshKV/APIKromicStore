using MediatR;

namespace KromicStore.Application.CustomerPortal.Commands.UpdateProfile;

/// <summary>
/// Command to update customer profile information.
/// </summary>
public sealed class UpdateProfileCommand : IRequest<UpdateProfileResponse>
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public sealed class UpdateProfileResponse
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime ModifiedOnUtc { get; set; }
}
