using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.ResolveContactRequest;

public sealed class ResolveContactRequestCommandHandler : IRequestHandler<ResolveContactRequestCommand, Unit>
{
    private readonly IContactRequestRepository _contactRepository;
    private readonly ILogger<ResolveContactRequestCommandHandler> _logger;

    public ResolveContactRequestCommandHandler(
        IContactRequestRepository contactRepository,
        ILogger<ResolveContactRequestCommandHandler> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(ResolveContactRequestCommand request, CancellationToken cancellationToken)
    {
        var contactRequest = await _contactRepository.GetByIdAsync(request.ContactRequestId, cancellationToken);
        if (contactRequest == null)
            throw new InvalidOperationException($"Contact request {request.ContactRequestId} not found.");

        contactRequest.Resolve(request.ResolvedByUserId, request.Notes);
        _contactRepository.Update(contactRequest);
        await _contactRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact request {ContactRequestId} resolved", request.ContactRequestId);
        return Unit.Value;
    }
}
