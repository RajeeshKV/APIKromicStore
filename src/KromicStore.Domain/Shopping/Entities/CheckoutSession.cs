using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// CheckoutSession aggregate root representing an in-progress checkout.
/// Manages billing/shipping addresses, shipping method, coupons, and payment state.
/// </summary>
public sealed class CheckoutSession : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid CustomerId { get; private set; }
    public Guid? BillingAddressId { get; private set; }
    public Guid? ShippingAddressId { get; private set; }
    public string? ShippingMethod { get; private set; }
    public string? PaymentMethod { get; private set; }
    public CheckoutSessionStatus Status { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }

    // Pricing
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal GrandTotal { get; private set; }

    // Applied coupon
    public string? CouponCode { get; private set; }

    // Relationships - stored as snapshots for historical accuracy
    private readonly List<CheckoutItem> _items = [];
    public IReadOnlyList<CheckoutItem> Items => _items.AsReadOnly();

    // Auditing and soft delete are inherited from AuditableEntity

    private CheckoutSession()
    {
    }

    private CheckoutSession(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    /// <summary>
    /// Create a new checkout session.
    /// </summary>
    public static CheckoutSession Create(Guid tenantId, Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));

        var session = new CheckoutSession(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            Status = CheckoutSessionStatus.Draft,
            ExpiresOnUtc = DateTime.UtcNow.AddHours(1) // 1-hour expiration
        };

        session.MarkCreated(DateTime.UtcNow, "System");

        return session;
    }

    /// <summary>
    /// Add an item to the checkout session.
    /// </summary>
    public void AddItem(Guid productId, decimal unitPrice, int quantity, Guid? variantId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (Status != CheckoutSessionStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a checkout session that is not in Draft status");

        var item = CheckoutItem.Create(productId, variantId, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Set billing address.
    /// </summary>
    public void SetBillingAddress(Guid addressId)
    {
        if (addressId == Guid.Empty)
            throw new ArgumentException("Address ID cannot be empty", nameof(addressId));

        BillingAddressId = addressId;
    }

    /// <summary>
    /// Set shipping address.
    /// </summary>
    public void SetShippingAddress(Guid addressId)
    {
        if (addressId == Guid.Empty)
            throw new ArgumentException("Address ID cannot be empty", nameof(addressId));

        ShippingAddressId = addressId;
    }

    /// <summary>
    /// Set shipping method and cost.
    /// </summary>
    public void SetShippingMethod(string method, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Shipping method cannot be empty", nameof(method));

        if (cost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(cost));

        ShippingMethod = method.Trim();
        ShippingAmount = cost;
        RecalculateTotals();
    }

    /// <summary>
    /// Apply a coupon code.
    /// </summary>
    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            throw new ArgumentException("Coupon code cannot be empty", nameof(couponCode));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        CouponCode = couponCode.Trim().ToUpperInvariant();
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    /// <summary>
    /// Remove applied coupon.
    /// </summary>
    public void RemoveCoupon()
    {
        CouponCode = null;
        DiscountAmount = 0;
        RecalculateTotals();
    }

    /// <summary>
    /// Set payment method.
    /// </summary>
    public void SetPaymentMethod(string method)
    {
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Payment method cannot be empty", nameof(method));

        PaymentMethod = method.Trim();
    }

    /// <summary>
    /// Transition to "awaiting payment" status.
    /// </summary>
    public void AwaitPayment()
    {
        if (Status != CheckoutSessionStatus.Draft)
            throw new InvalidOperationException("Can only transition to AwaitingPayment from Draft status");

        if (!BillingAddressId.HasValue || !ShippingAddressId.HasValue)
            throw new InvalidOperationException("Billing and shipping addresses must be set before payment");

        if (string.IsNullOrEmpty(ShippingMethod))
            throw new InvalidOperationException("Shipping method must be selected");

        Status = CheckoutSessionStatus.AwaitingPayment;
    }

    /// <summary>
    /// Mark checkout as completed.
    /// </summary>
    public void Complete()
    {
        if (Status != CheckoutSessionStatus.AwaitingPayment)
            throw new InvalidOperationException("Can only complete from AwaitingPayment status");

        Status = CheckoutSessionStatus.Completed;
    }

    /// <summary>
    /// Mark checkout as expired.
    /// </summary>
    public void Expire()
    {
        if (Status == CheckoutSessionStatus.Completed || Status == CheckoutSessionStatus.Expired)
            throw new InvalidOperationException("Cannot expire a completed or already expired checkout");

        Status = CheckoutSessionStatus.Expired;
    }

    /// <summary>
    /// Cancel the checkout session.
    /// </summary>
    public void Cancel()
    {
        if (Status == CheckoutSessionStatus.Completed || Status == CheckoutSessionStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled checkout");

        Status = CheckoutSessionStatus.Cancelled;
    }

    /// <summary>
    /// Check if checkout session has expired.
    /// </summary>
    public bool IsExpired => ExpiresOnUtc.HasValue && DateTime.UtcNow > ExpiresOnUtc.Value;

    /// <summary>
    /// Recalculate all totals based on items, discounts, shipping, and tax.
    /// </summary>
    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.LineTotal);
        GrandTotal = SubTotal - DiscountAmount + ShippingAmount + TaxAmount;

        if (GrandTotal < 0)
            GrandTotal = 0;
    }
}

/// <summary>
/// Checkout session status enumeration.
/// </summary>
public enum CheckoutSessionStatus
{
    Draft = 0,
    AwaitingPayment = 1,
    Completed = 2,
    Expired = 3,
    Cancelled = 4
}
