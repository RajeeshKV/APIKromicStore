using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.CreateContactRequest;

public sealed class CreateContactRequestCommand : IRequest<CreateContactRequestResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Category { get; set; }
}

public sealed class CreateContactRequestResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
}
