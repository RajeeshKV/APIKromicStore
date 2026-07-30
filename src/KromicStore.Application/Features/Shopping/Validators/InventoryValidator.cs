namespace KromicStore.Application.Features.Shopping.Validators;

/// <summary>
/// Reusable validator for inventory validation.
/// Validates quantities and stock availability constraints.
/// </summary>
public sealed class InventoryValidator
{
    private const int MinimumQuantity = 1;
    private const int MaximumQuantity = 1000;

    /// <summary>
    /// Validates product quantity is within acceptable range.
    /// </summary>
    public static bool IsValidQuantity(int quantity)
    {
        return quantity >= MinimumQuantity && quantity <= MaximumQuantity;
    }

    /// <summary>
    /// Gets validation error for invalid quantity.
    /// </summary>
    public static string GetQuantityError(int quantity)
    {
        if (quantity < MinimumQuantity)
            return $"Quantity must be at least {MinimumQuantity}";

        if (quantity > MaximumQuantity)
            return $"Quantity cannot exceed {MaximumQuantity}";

        return string.Empty;
    }

    /// <summary>
    /// Validates requested quantity against available stock.
    /// </summary>
    public static bool HasSufficientStock(int requestedQuantity, int availableStock)
    {
        return requestedQuantity > 0 && requestedQuantity <= availableStock;
    }

    /// <summary>
    /// Gets validation error for insufficient stock.
    /// </summary>
    public static string GetInsufficientStockError(int requestedQuantity, int availableStock)
    {
        if (requestedQuantity <= 0)
            return "Quantity must be greater than 0";

        if (requestedQuantity > availableStock)
            return $"Only {availableStock} items available in stock";

        return string.Empty;
    }

    /// <summary>
    /// Validates unit price is within acceptable range.
    /// </summary>
    public static bool IsValidUnitPrice(decimal unitPrice)
    {
        return unitPrice >= 0 && unitPrice <= decimal.MaxValue / 1000;
    }

    /// <summary>
    /// Gets validation error for invalid unit price.
    /// </summary>
    public static string GetUnitPriceError(decimal unitPrice)
    {
        if (unitPrice < 0)
            return "Unit price cannot be negative";

        if (unitPrice > decimal.MaxValue / 1000)
            return "Unit price exceeds maximum allowed value";

        return string.Empty;
    }
}
