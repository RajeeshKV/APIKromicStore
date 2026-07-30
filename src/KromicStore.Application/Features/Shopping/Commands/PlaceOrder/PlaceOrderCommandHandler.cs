using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.PlaceOrder;

/// <summary>
/// Handler for PlaceOrder command.
/// Places an order from a checkout session with confirmed payment.
/// </summary>
public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResponse>
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public PlaceOrderCommandHandler(
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<PlaceOrderCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Placing order from checkout session {CheckoutSessionId} with transaction {TransactionId}", command.CheckoutSessionId, command.PaymentTransactionId);

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

        // Verify checkout session is in awaiting payment state
        if (checkoutSession.Status.ToString() != "AwaitingPayment")
        {
            _logger.LogWarning("Cannot place order for checkout session not in AwaitingPayment state: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Order can only be placed for checkout sessions in AwaitingPayment state");
        }

        // Verify payment has been initialized
        if (string.IsNullOrWhiteSpace(checkoutSession.PaymentMethod))
        {
            _logger.LogWarning("Payment not initialized for checkout session: {CheckoutSessionId}", command.CheckoutSessionId);
            throw new InvalidOperationException("Payment must be initialized before placing an order");
        }

        // TODO: In a real implementation, verify payment status with payment gateway
        // For now, we'll assume payment was successful

        // Complete checkout and create order
        checkoutSession.Complete();

        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{checkoutSession.Id.ToString().Substring(0, 8).ToUpper()}";

        // TODO: Create Order entity in domain
        // For now, we're just marking checkout as completed

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order placed successfully from checkout session: {CheckoutSessionId}", command.CheckoutSessionId);

        // Map items
        var items = checkoutSession.Items.Select(i => new OrderItemDto(
            ProductId: i.ProductId,
            VariantId: i.ProductVariantId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            LineTotal: i.LineTotal)).ToList();

        return new PlaceOrderResponse(
            OrderId: checkoutSession.Id, // Using checkout session ID as order ID for now
            CustomerId: checkoutSession.CustomerId,
            OrderNumber: orderNumber,
            Items: items,
            SubTotal: checkoutSession.SubTotal,
            ShippingCost: checkoutSession.ShippingAmount,
            DiscountAmount: checkoutSession.DiscountAmount,
            Total: checkoutSession.GrandTotal,
            Status: checkoutSession.Status.ToString(),
            CreatedOnUtc: checkoutSession.CreatedOnUtc);
    }
}
