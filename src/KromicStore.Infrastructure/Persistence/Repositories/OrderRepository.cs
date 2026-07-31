using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order aggregate root.
/// Enforces tenant isolation and provides data access operations.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public OrderRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Timeline)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return null;

        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Timeline)
            .Include(o => o.OrderNotes)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .Include(o => o.Timeline)
            .Include(o => o.OrderNotes)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .Include(o => o.Timeline)
            .Include(o => o.OrderNotes)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAndStatusAsync(
        Guid customerId,
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId && o.Status == status)
            .Include(o => o.Items)
            .Include(o => o.Timeline)
            .Include(o => o.OrderNotes)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingOrderAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return false;

        return await _dbContext.Orders
            .AnyAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(
        Guid customerId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
            return [];

        if (limit <= 0)
            limit = 10;

        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedOnUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return false;

        return await _dbContext.Orders
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public void Add(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _dbContext.OrderSet.Add(order);
    }

    public void Update(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _dbContext.OrderSet.Update(order);
    }

    public void Remove(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _dbContext.OrderSet.Remove(order);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
