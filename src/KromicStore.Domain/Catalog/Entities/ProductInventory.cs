using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductInventory value object managing inventory for a product or variant.
/// Tracks available quantity, reserved quantity, and reorder level.
/// Available stock = AvailableQuantity - ReservedQuantity
/// </summary>
public sealed class ProductInventory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public DateTime LastAdjustedUtc { get; private set; }

    private ProductInventory()
    {
    }

    private ProductInventory(Guid id) : base(id)
    {
    }

    public static ProductInventory Create(
        Guid productId,
        bool trackInventory,
        Guid? variantId = null,
        int availableQuantity = 0,
        int reorderLevel = 10)
    {
        if (!trackInventory)
            availableQuantity = 999999; // Unlimited for non-tracked products

        if (availableQuantity < 0)
            throw new ArgumentException("Available quantity cannot be negative", nameof(availableQuantity));

        if (reorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(reorderLevel));

        var inventory = new ProductInventory(Guid.NewGuid())
        {
            ProductId = productId,
            VariantId = variantId,
            AvailableQuantity = availableQuantity,
            ReservedQuantity = 0,
            ReorderLevel = reorderLevel,
            LastAdjustedUtc = DateTime.UtcNow
        };

        return inventory;
    }

    public int GetAvailableStock() => AvailableQuantity - ReservedQuantity;

    public bool IsLowStock() => GetAvailableStock() <= ReorderLevel;

    public void AdjustAvailableQuantity(int change, string reason)
    {
        if (AvailableQuantity + change < 0)
            throw new InvalidOperationException($"Cannot reduce available quantity below 0. Current: {AvailableQuantity}, Change: {change}");

        AvailableQuantity += change;
        LastAdjustedUtc = DateTime.UtcNow;
    }

    public void ReserveInventory(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reserve quantity must be greater than 0", nameof(quantity));

        var availableStock = GetAvailableStock();
        if (quantity > availableStock)
            throw new InvalidOperationException($"Cannot reserve {quantity} units. Available: {availableStock}");

        ReservedQuantity += quantity;
        LastAdjustedUtc = DateTime.UtcNow;
    }

    public void ReleaseReservedInventory(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Release quantity must be greater than 0", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException($"Cannot release {quantity} units. Reserved: {ReservedQuantity}");

        ReservedQuantity -= quantity;
        LastAdjustedUtc = DateTime.UtcNow;
    }

    public void UpdateReorderLevel(int newLevel)
    {
        if (newLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(newLevel));

        ReorderLevel = newLevel;
    }
}
