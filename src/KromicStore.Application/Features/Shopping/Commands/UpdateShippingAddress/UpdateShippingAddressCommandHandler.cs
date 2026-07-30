using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateShippingAddress;

/// <summary>
/// Handler for UpdateShippingAddress command.
/// Updates the shipping address for a checkout session.
/// </summary>
public sealed class UpdateShippingAddressCommandHandler : IRequestHandler<UpdateShippingAddressCommand, UpdateShippingAddressResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateShippingAddressCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public UpdateShippingAddressCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateShippingAddressCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<UpdateShippingAddressResponse> Handle(UpdateShippingAddressCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating shipping address for checkout session {CheckoutSessionId}", command.CheckoutSessionId);

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

        // Update shipping address
        checkoutSession.SetShippingAddress(addressId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shipping address updated for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        var fullAddress = $"{command.Street}, {command.City}, {command.State} {command.PostalCode}, {command.Country}";

        return new UpdateShippingAddressResponse(
            CheckoutSessionId: checkoutSession.Id,
            ShippingAddressUpdated: true,
            FullAddress: fullAddress);
    }
}
