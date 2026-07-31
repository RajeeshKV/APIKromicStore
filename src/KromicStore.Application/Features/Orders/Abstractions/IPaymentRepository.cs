using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Abstractions;

/// <summary>
/// Repository abstraction for Payment aggregate root.
/// Enforces tenant isolation and payment state management.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Get payment by ID.
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment by order ID.
    /// </summary>
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments for a customer.
    /// </summary>
    Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments by status.
    /// </summary>
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments scheduled for retry.
    /// </summary>
    Task<IEnumerable<Payment>> GetRetryScheduledPaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if payment exists for order.
    /// </summary>
    Task<bool> PaymentExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment by provider transaction ID.
    /// </summary>
    Task<Payment?> GetByProviderTransactionIdAsync(string providerTransactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get failed payments for a customer.
    /// </summary>
    Task<IEnumerable<Payment>> GetFailedPaymentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new payment to the repository.
    /// </summary>
    void Add(Payment payment);

    /// <summary>
    /// Update an existing payment.
    /// </summary>
    void Update(Payment payment);

    /// <summary>
    /// Remove/delete a payment.
    /// </summary>
    void Remove(Payment payment);

    /// <summary>
    /// Save changes to the repository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
