using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.InitializePayment;

/// <summary>
/// Handler for InitializePayment command.
/// Initializes payment processing for a checkout session.
/// </summary>
public sealed class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, InitializePaymentResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<InitializePaymentCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public InitializePaymentCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<InitializePaymentCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<InitializePaymentResponse> Handle(InitializePaymentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing payment for checkout session {CheckoutSessionId} with method {PaymentMethod}", command.CheckoutSessionId, command.PaymentMethod);

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
            _logger.LogWarning("Cannot initialize payment for checkout session not in Draft state: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Payment can only be initialized for checkout sessions in Draft state");
        }

        // Verify required checkout data is present
        if (!checkoutSession.BillingAddressId.HasValue)
        {
            _logger.LogWarning("Billing address not set for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Billing address must be set before initializing payment");
        }

        if (!checkoutSession.ShippingAddressId.HasValue)
        {
            _logger.LogWarning("Shipping address not set for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Shipping address must be set before initializing payment");
        }

        if (string.IsNullOrWhiteSpace(checkoutSession.ShippingMethod))
        {
            _logger.LogWarning("Shipping method not selected for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Shipping method must be selected before initializing payment");
        }

        // Initialize payment (in real implementation, this would call payment gateway)
        // For now, set payment method and transition to AwaitingPayment status
        checkoutSession.SetPaymentMethod(command.PaymentMethod);
        checkoutSession.AwaitPayment();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment initialized for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        return new InitializePaymentResponse(
            CheckoutSessionId: checkoutSession.Id,
            PaymentMethod: checkoutSession.PaymentMethod ?? command.PaymentMethod,
            Amount: checkoutSession.GrandTotal,
            PaymentStatus: checkoutSession.Status.ToString(),
            PaymentToken: Guid.NewGuid().ToString());
    }
}
