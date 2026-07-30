namespace KromicStore.Application.Features.Shopping.Validators;

/// <summary>
/// Reusable validator for shipping method validation.
/// Validates shipping method selection and cost constraints.
/// </summary>
public sealed class ShippingMethodValidator
{
    private const int ShippingMethodIdMaxLength = 100;
    private static readonly string[] ValidShippingMethods = { "Standard", "Express", "Overnight", "Pickup" };

    /// <summary>
    /// Validates shipping method ID format.
    /// </summary>
    public static bool IsValidShippingMethodId(string shippingMethodId)
    {
        if (string.IsNullOrWhiteSpace(shippingMethodId))
            return false;

        if (shippingMethodId.Length > ShippingMethodIdMaxLength)
            return false;

        return true;
    }

    /// <summary>
    /// Gets validation error for invalid shipping method ID.
    /// </summary>
    public static string GetShippingMethodIdError(string shippingMethodId)
    {
        if (string.IsNullOrWhiteSpace(shippingMethodId))
            return "Shipping method ID is required";

        if (shippingMethodId.Length > ShippingMethodIdMaxLength)
            return $"Shipping method ID cannot exceed {ShippingMethodIdMaxLength} characters";

        return string.Empty;
    }

    /// <summary>
    /// Validates shipping cost is within acceptable range.
    /// </summary>
    public static bool IsValidShippingCost(decimal shippingCost)
    {
        return shippingCost >= 0 && shippingCost <= decimal.MaxValue / 1000;
    }

    /// <summary>
    /// Gets validation error for invalid shipping cost.
    /// </summary>
    public static string GetShippingCostError(decimal shippingCost)
    {
        if (shippingCost < 0)
            return "Shipping cost cannot be negative";

        if (shippingCost > decimal.MaxValue / 1000)
            return "Shipping cost exceeds maximum allowed value";

        return string.Empty;
    }

    /// <summary>
    /// Checks if a shipping method is known/supported.
    /// This is a simple check and could be extended to query a repository.
    /// </summary>
    public static bool IsSupportedShippingMethod(string shippingMethodId)
    {
        return ValidShippingMethods.Contains(shippingMethodId) || 
               (!string.IsNullOrWhiteSpace(shippingMethodId) && Guid.TryParse(shippingMethodId, out _));
    }

    /// <summary>
    /// Validates that shipping address is set before selecting shipping method.
    /// </summary>
    public static bool CanSelectShippingMethod(bool hasShippingAddress)
    {
        return hasShippingAddress;
    }

    /// <summary>
    /// Gets validation error when shipping address is not set.
    /// </summary>
    public static string GetShippingAddressRequiredError()
    {
        return "Shipping address must be set before selecting a shipping method";
    }
}
