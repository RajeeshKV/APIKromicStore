using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Orders.Entities;
using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICheckoutSessionRepository _checkoutRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICheckoutSessionRepository checkoutRepository,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _checkoutRepository = checkoutRepository ?? throw new ArgumentNullException(nameof(checkoutRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Retrieve checkout session
        var checkoutSession = await _checkoutRepository.GetByIdAsync(request.CheckoutSessionId, cancellationToken);
        if (checkoutSession == null)
            throw new InvalidOperationException($"Checkout session {request.CheckoutSessionId} not found");

        // Validate checkout session status
        if (checkoutSession.Status != CheckoutSessionStatus.AwaitingPayment && 
            checkoutSession.Status != CheckoutSessionStatus.Draft)
            throw new InvalidOperationException($"Cannot create order from checkout session in {checkoutSession.Status} status");

        // Validate customer ownership
        if (checkoutSession.CustomerId != request.CustomerId)
            throw new UnauthorizedAccessException("Checkout session does not belong to this customer");

        // Validate checkout has items
        if (checkoutSession.Items.Count == 0)
            throw new InvalidOperationException("Cannot create order from empty checkout session");

        // Generate order number
        var orderNumber = GenerateOrderNumber(request.TenantId);

        // Check for duplicates
        if (await _orderRepository.OrderNumberExistsAsync(orderNumber, cancellationToken))
            throw new InvalidOperationException($"Order number {orderNumber} already exists");

        // Create order items
        var orderItems = checkoutSession.Items
            .Select(ci => OrderItem.Create(
                orderId: Guid.NewGuid(), // Will be set when adding to Order
                productId: ci.ProductId,
                productName: "Product", // TODO: Get from product repository
                productSku: "SKU",      // TODO: Get from product repository
                quantity: ci.Quantity,
                unitPrice: ci.UnitPrice,
                variantId: ci.ProductVariantId,
                variantName: null))
            .ToList();

        // Create order
        var order = Order.Create(
            tenantId: request.TenantId,
            customerId: request.CustomerId,
            orderNumber: orderNumber,
            billingAddressId: checkoutSession.BillingAddressId ?? Guid.Empty,
            shippingAddressId: checkoutSession.ShippingAddressId ?? Guid.Empty,
            shippingMethod: checkoutSession.ShippingMethod ?? "Standard",
            paymentMethod: checkoutSession.PaymentMethod ?? "Unknown",
            items: orderItems,
            subTotal: checkoutSession.SubTotal,
            discountAmount: checkoutSession.DiscountAmount,
            shippingAmount: checkoutSession.ShippingAmount,
            taxAmount: checkoutSession.TaxAmount,
            couponCode: checkoutSession.CouponCode);

        // Add to repository
        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} created from checkout session {CheckoutSessionId}", 
            orderNumber, request.CheckoutSessionId);

        return new CreateOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CreatedOnUtc = order.CreatedOnUtc,
            GrandTotal = order.GrandTotal,
            Status = order.Status.ToString()
        };
    }

    private string GenerateOrderNumber(Guid tenantId)
    {
        // Format: ORD-YYYYMMDD-XXXXXXXX (timestamp-based)
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
