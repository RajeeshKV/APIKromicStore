using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.SelectShippingMethod;

/// <summary>
/// Handler for SelectShippingMethod command.
/// Selects a shipping method and calculates shipping cost for checkout session.
/// </summary>
public sealed class SelectShippingMethodCommandHandler : IRequestHandler<SelectShippingMethodCommand, SelectShippingMethodResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SelectShippingMethodCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public SelectShippingMethodCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<SelectShippingMethodCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<SelectShippingMethodResponse> Handle(SelectShippingMethodCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Selecting shipping method {ShippingMethodId} for checkout session {CheckoutSessionId}", command.ShippingMethodId, command.CheckoutSessionId);

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

        // Verify checkout session is in draft state
        if (checkoutSession.Status.ToString() != "Draft")
        {
            _logger.LogWarning("Cannot select shipping method for checkout session not in Draft state: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Shipping method can only be selected for checkout sessions in Draft state");
        }

        // Verify shipping address is set
        if (!checkoutSession.ShippingAddressId.HasValue)
        {
            _logger.LogWarning("Shipping address not set for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Shipping address must be set before selecting a shipping method");
        }

        // Select shipping method and cost
        checkoutSession.SetShippingMethod(command.ShippingMethodId, command.ShippingCost);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shipping method selected for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        return new SelectShippingMethodResponse(
            CheckoutSessionId: checkoutSession.Id,
            ShippingMethodId: checkoutSession.ShippingMethod ?? "",
            ShippingCost: checkoutSession.ShippingAmount,
            Total: checkoutSession.GrandTotal);
    }
}
