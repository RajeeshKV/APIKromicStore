using System.Text.RegularExpressions;

namespace KromicStore.Application.Features.Shopping.Validators;

/// <summary>
/// Reusable validator for coupon code validation.
/// Ensures coupon codes follow expected format and constraints.
/// </summary>
public sealed class CouponValidator
{
    private const int CouponCodeMinLength = 3;
    private const int CouponCodeMaxLength = 50;
    private static readonly Regex CouponCodePattern = new(@"^[A-Z0-9\-]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates coupon code format.
    /// </summary>
    public static bool IsValidCouponCode(string couponCode)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return false;

        if (couponCode.Length < CouponCodeMinLength || couponCode.Length > CouponCodeMaxLength)
            return false;

        return CouponCodePattern.IsMatch(couponCode);
    }

    /// <summary>
    /// Gets validation error for invalid coupon code.
    /// </summary>
    public static string GetValidationError(string couponCode)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return "Coupon code is required";

        if (couponCode.Length < CouponCodeMinLength)
            return $"Coupon code must be at least {CouponCodeMinLength} characters";

        if (couponCode.Length > CouponCodeMaxLength)
            return $"Coupon code cannot exceed {CouponCodeMaxLength} characters";

        if (!CouponCodePattern.IsMatch(couponCode))
            return "Coupon code can only contain uppercase letters, numbers, and hyphens";

        return string.Empty;
    }

    /// <summary>
    /// Validates discount amount is within acceptable range.
    /// </summary>
    public static bool IsValidDiscountAmount(decimal discountAmount, decimal subtotal)
    {
        if (discountAmount < 0)
            return false;

        if (discountAmount > subtotal)
            return false;

        return true;
    }

    /// <summary>
    /// Gets validation error for invalid discount amount.
    /// </summary>
    public static string GetDiscountAmountError(decimal discountAmount, decimal subtotal)
    {
        if (discountAmount < 0)
            return "Discount amount cannot be negative";

        if (discountAmount > subtotal)
            return "Discount amount cannot exceed subtotal";

        return string.Empty;
    }
}
