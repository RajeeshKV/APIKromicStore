using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateBillingAddress;

/// <summary>
/// Handler for UpdateBillingAddress command.
/// Updates the billing address for a checkout session.
/// </summary>
public sealed class UpdateBillingAddressCommandHandler : IRequestHandler<UpdateBillingAddressCommand, UpdateBillingAddressResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateBillingAddressCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public UpdateBillingAddressCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateBillingAddressCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<UpdateBillingAddressResponse> Handle(UpdateBillingAddressCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating billing address for checkout session {CheckoutSessionId}", command.CheckoutSessionId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        var checkoutSession = await _checkoutSessionRepository.GetByIdAsync(command.CheckoutSessionId, cancellationToken);
        if (checkoutSession == null)
        {
            _logger.LogWarning("Checkout session not found: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException($"Checkout session with ID {command.CheckoutSessionId} not found");
        }

        if (checkoutSession.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new UnauthorizedAccessException("Cannot access checkout session from another tenant");
        }

        // TODO: Store address in a separate Address entity/table
        // For now, generate a Guid to represent the address
        var addressId = Guid.NewGuid();

        // Update billing address
        checkoutSession.SetBillingAddress(addressId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Billing address updated for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        var fullAddress = $"{command.Street}, {command.City}, {command.State} {command.PostalCode}, {command.Country}";

        return new UpdateBillingAddressResponse(
            CheckoutSessionId: checkoutSession.Id,
            BillingAddressUpdated: true,
            FullAddress: fullAddress);
    }
}
