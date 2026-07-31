using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.CreateContactRequest;

public sealed class CreateContactRequestCommandHandler : IRequestHandler<CreateContactRequestCommand, CreateContactRequestResponse>
{
    private readonly IContactRequestRepository _contactRepository;
    private readonly ILogger<CreateContactRequestCommandHandler> _logger;

    public CreateContactRequestCommandHandler(
        IContactRequestRepository contactRepository,
        ILogger<CreateContactRequestCommandHandler> logger)
    {
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateContactRequestResponse> Handle(
        CreateContactRequestCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating contact request from {Email}", request.Email);

        var contactRequest = ContactRequest.Create(
            request.Name,
            request.Email,
            request.Subject,
            request.Message,
            request.PhoneNumber,
            request.Category);

        _contactRepository.Add(contactRequest);
        await _contactRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact request {ContactRequestId} created successfully", contactRequest.Id);

        return new CreateContactRequestResponse
        {
            Id = contactRequest.Id,
            Status = contactRequest.Status.ToString()
        };
    }
}
