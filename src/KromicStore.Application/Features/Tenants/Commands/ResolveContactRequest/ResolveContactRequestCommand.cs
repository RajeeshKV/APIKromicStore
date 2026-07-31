using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.ResolveContactRequest;

public sealed class ResolveContactRequestCommand : IRequest<Unit>
{
    public Guid ContactRequestId { get; set; }
    public Guid ResolvedByUserId { get; set; }
    public string? Notes { get; set; }
}
