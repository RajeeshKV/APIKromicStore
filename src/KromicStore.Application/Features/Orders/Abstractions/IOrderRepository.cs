using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Abstractions;

/// <summary>
/// Repository abstraction for Order aggregate root.
/// Enforces tenant isolation and business rule validation.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Get order by ID.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order by order number.
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders for a customer.
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status.
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders for a customer with specific status.
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerIdAndStatusAsync(
        Guid customerId,
        OrderStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer has an active order.
    /// </summary>
    Task<bool> HasPendingOrderAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent orders for a customer (pagination).
    /// </summary>
    Task<IEnumerable<Order>> GetRecentOrdersAsync(
        Guid customerId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if order number already exists.
    /// </summary>
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new order to the repository.
    /// </summary>
    void Add(Order order);

    /// <summary>
    /// Update an existing order.
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Remove/delete an order.
    /// </summary>
    void Remove(Order order);

    /// <summary>
    /// Save changes to the repository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders for a tenant (used for platform analytics).
    /// </summary>
    Task<IEnumerable<Order>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of all orders in the system.
    /// </summary>
    Task<int> GetTotalOrderCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of orders for a specific tenant.
    /// </summary>
    Task<int> GetOrderCountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total revenue across all orders in the system.
    /// </summary>
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total revenue for a specific tenant.
    /// </summary>
    Task<decimal> GetRevenueBytTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get distinct customer count across all orders in the system.
    /// </summary>
    Task<int> GetTotalUniqueCustomerCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get distinct customer count for a specific tenant.
    /// </summary>
    Task<int> GetUniqueCustomerCountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
