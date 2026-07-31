using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Commands.InitializePayment;

public sealed class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, InitializePaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<InitializePaymentCommandHandler> _logger;

    public InitializePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ILogger<InitializePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InitializePaymentResponse> Handle(InitializePaymentCommand request, CancellationToken cancellationToken)
    {
        // Retrieve order to validate it exists
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.OrderId} not found");

        // Validate order belongs to customer
        if (order.CustomerId != request.CustomerId)
            throw new UnauthorizedAccessException("Order does not belong to this customer");

        // Check if payment already exists for this order
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingPayment != null)
            throw new InvalidOperationException($"Payment already exists for order {request.OrderId}");

        // Create payment
        var payment = Payment.Create(
            tenantId: request.TenantId,
            orderId: request.OrderId,
            customerId: request.CustomerId,
            paymentMethod: request.PaymentMethod,
            amount: request.Amount,
            currency: request.Currency,
            provider: request.Provider);

        // Link payment to order
        order.LinkPayment(payment.Id);

        // Add payment
        _paymentRepository.Add(payment);
        _orderRepository.Update(order);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} initialized for order {OrderId}", payment.Id, order.OrderNumber);

        return new InitializePaymentResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status.ToString(),
            InitiatedOnUtc = payment.InitiatedOnUtc
        };
    }
}
