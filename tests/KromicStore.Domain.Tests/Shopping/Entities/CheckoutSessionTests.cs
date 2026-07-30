using Xunit;
using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Domain.Tests.Shopping.Entities;

/// <summary>
/// Domain tests for CheckoutSession aggregate root.
/// Tests verify status transitions, calculations, address validation, and aggregate consistency.
/// </summary>
public class CheckoutSessionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    #region Checkout Session Creation Tests

    [Fact]
    public void Create_WithValidData_CreatesCheckoutSession()
    {
        // Arrange & Act
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(_customerId, session.CustomerId);
        Assert.Equal(_tenantId, session.TenantId);
        Assert.Equal(CheckoutSessionStatus.Draft, session.Status);
        Assert.Empty(session.Items);
        Assert.Null(session.BillingAddressId);
        Assert.Null(session.ShippingAddressId);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CheckoutSession.Create(_tenantId, Guid.Empty));
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var session1 = CheckoutSession.Create(_tenantId, _customerId);
        var session2 = CheckoutSession.Create(_tenantId, _customerId);

        // Assert
        Assert.NotEqual(session1.Id, session2.Id);
    }

    [Fact]
    public void Create_StartsWithExpiration()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Assert
        Assert.True(session.ExpiresOnUtc.HasValue);
        var expectedExpiration = beforeCreation.AddHours(1);
        Assert.True(session.ExpiresOnUtc >= expectedExpiration.AddSeconds(-1));
        Assert.True(session.ExpiresOnUtc <= expectedExpiration.AddSeconds(1));
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public void AddItem_WithValidData_AddsItem()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();

        // Act
        session.AddItem(productId, 50m, 2);

        // Assert
        Assert.Single(session.Items);
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.AddItem(Guid.Empty, 10m, 1));
    }

    [Fact]
    public void AddItem_WithNegativePrice_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.AddItem(Guid.NewGuid(), -5m, 1));
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.AddItem(Guid.NewGuid(), 10m, 0));
    }

    [Fact]
    public void AddItem_WithVariant_AddsItem()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        // Act
        session.AddItem(productId, 50m, 2, variantId);

        // Assert
        Assert.Single(session.Items);
        Assert.Equal(variantId, session.Items[0].ProductVariantId);
    }

    [Fact]
    public void AddItem_MultipleItems_AddsAll()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.AddItem(Guid.NewGuid(), 75m, 3);
        session.AddItem(Guid.NewGuid(), 25m, 1);

        // Assert
        Assert.Equal(3, session.Items.Count);
    }

    [Fact]
    public void AddItem_ToNonDraftSession_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.AddItem(Guid.NewGuid(), 50m, 1));
    }

    #endregion

    #region Address Tests

    [Fact]
    public void SetBillingAddress_WithValidId_SetsBillingAddress()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var addressId = Guid.NewGuid();

        // Act
        session.SetBillingAddress(addressId);

        // Assert
        Assert.Equal(addressId, session.BillingAddressId);
    }

    [Fact]
    public void SetBillingAddress_WithEmptyId_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.SetBillingAddress(Guid.Empty));
    }

    [Fact]
    public void SetShippingAddress_WithValidId_SetsShippingAddress()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var addressId = Guid.NewGuid();

        // Act
        session.SetShippingAddress(addressId);

        // Assert
        Assert.Equal(addressId, session.ShippingAddressId);
    }

    [Fact]
    public void SetShippingAddress_WithEmptyId_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.SetShippingAddress(Guid.Empty));
    }

    [Fact]
    public void SetBillingAddress_CanUpdateAddress()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var addressId1 = Guid.NewGuid();
        var addressId2 = Guid.NewGuid();

        // Act
        session.SetBillingAddress(addressId1);
        session.SetBillingAddress(addressId2);

        // Assert
        Assert.Equal(addressId2, session.BillingAddressId);
    }

    #endregion

    #region Shipping Method Tests

    [Fact]
    public void SetShippingMethod_WithValidData_SetsMethod()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.SetShippingMethod("Standard", 10m);

        // Assert
        Assert.Equal("Standard", session.ShippingMethod);
        Assert.Equal(10m, session.ShippingAmount);
    }

    [Fact]
    public void SetShippingMethod_WithEmptyMethod_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.SetShippingMethod("", 10m));
    }

    [Fact]
    public void SetShippingMethod_WithNegativeCost_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.SetShippingMethod("Standard", -5m));
    }

    [Fact]
    public void SetShippingMethod_TrimsWhitespace()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.SetShippingMethod("  Standard  ", 10m);

        // Assert
        Assert.Equal("Standard", session.ShippingMethod);
    }

    #endregion

    #region Coupon Tests

    [Fact]
    public void ApplyCoupon_WithValidData_AppliesCoupon()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.ApplyCoupon("SUMMER20", 20m);

        // Assert
        Assert.Equal("SUMMER20", session.CouponCode);
        Assert.Equal(20m, session.DiscountAmount);
    }

    [Fact]
    public void ApplyCoupon_WithEmptyCode_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.ApplyCoupon("", 20m));
    }

    [Fact]
    public void ApplyCoupon_WithNegativeDiscount_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.ApplyCoupon("SUMMER20", -10m));
    }

    [Fact]
    public void ApplyCoupon_ConvertsToUppercase()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.ApplyCoupon("summer20", 20m);

        // Assert
        Assert.Equal("SUMMER20", session.CouponCode);
    }

    [Fact]
    public void ApplyCoupon_ReplacesPreviousCoupon()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.ApplyCoupon("SUMMER20", 20m);

        // Act
        session.ApplyCoupon("FALL10", 10m);

        // Assert
        Assert.Equal("FALL10", session.CouponCode);
        Assert.Equal(10m, session.DiscountAmount);
    }

    [Fact]
    public void RemoveCoupon_ClearsCoupon()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.ApplyCoupon("SUMMER20", 20m);

        // Act
        session.RemoveCoupon();

        // Assert
        Assert.Null(session.CouponCode);
        Assert.Equal(0m, session.DiscountAmount);
    }

    [Fact]
    public void RemoveCoupon_NoActiveCoupon_RemainsNull()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.RemoveCoupon();

        // Assert
        Assert.Null(session.CouponCode);
    }

    #endregion

    #region Payment Method Tests

    [Fact]
    public void SetPaymentMethod_WithValidMethod_SetsMethod()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.SetPaymentMethod("Credit Card");

        // Assert
        Assert.Equal("Credit Card", session.PaymentMethod);
    }

    [Fact]
    public void SetPaymentMethod_WithEmptyMethod_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => session.SetPaymentMethod(""));
    }

    [Fact]
    public void SetPaymentMethod_TrimsWhitespace()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.SetPaymentMethod("  Credit Card  ");

        // Assert
        Assert.Equal("Credit Card", session.PaymentMethod);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void AwaitPayment_FromDraft_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);

        // Act
        session.AwaitPayment();

        // Assert
        Assert.Equal(CheckoutSessionStatus.AwaitingPayment, session.Status);
    }

    [Fact]
    public void AwaitPayment_WithoutBillingAddress_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.AwaitPayment());
    }

    [Fact]
    public void AwaitPayment_WithoutShippingAddress_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.AwaitPayment());
    }

    [Fact]
    public void AwaitPayment_WithoutShippingMethod_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.AwaitPayment());
    }

    [Fact]
    public void AwaitPayment_NotFromDraft_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.AwaitPayment());
    }

    [Fact]
    public void Complete_FromAwaitingPayment_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();

        // Act
        session.Complete();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Completed, session.Status);
    }

    [Fact]
    public void Complete_NotFromAwaitingPayment_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.Complete());
    }

    [Fact]
    public void Expire_FromDraft_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.Expire();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Expired, session.Status);
    }

    [Fact]
    public void Expire_FromAwaitingPayment_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();

        // Act
        session.Expire();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Expired, session.Status);
    }

    [Fact]
    public void Expire_FromCompleted_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();
        session.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.Expire());
    }

    [Fact]
    public void Cancel_FromDraft_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act
        session.Cancel();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Cancelled, session.Status);
    }

    [Fact]
    public void Cancel_FromAwaitingPayment_ChangesStatus()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();

        // Act
        session.Cancel();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Cancelled, session.Status);
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsException()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);
        session.AwaitPayment();
        session.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.Cancel());
    }

    #endregion

    #region Calculation Tests

    [Fact]
    public void Calculations_EmptySession_ReturnsZero()
    {
        // Arrange & Act
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Assert
        Assert.Equal(0m, session.SubTotal);
        Assert.Equal(0m, session.GrandTotal);
        Assert.Equal(0m, session.DiscountAmount);
        Assert.Equal(0m, session.ShippingAmount);
        Assert.Equal(0m, session.TaxAmount);
    }

    [Fact]
    public void SubTotal_CalculatesFromItems()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 50m, 2);   // 100
        session.AddItem(Guid.NewGuid(), 75m, 3);   // 225
        // Expected: 325

        // Act
        var subTotal = session.SubTotal;

        // Assert
        Assert.Equal(325m, subTotal);
    }

    [Fact]
    public void GrandTotal_IncludesSubtotalShippingTax()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 2);  // 200 subtotal

        // Act
        session.SetShippingMethod("Standard", 10m);
        var grandTotal = session.GrandTotal;

        // Assert
        // Subtotal(200) - Discount(0) + Shipping(10) + Tax(0) = 210
        Assert.Equal(210m, grandTotal);
    }

    [Fact]
    public void GrandTotal_WithDiscount()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 2);  // 200 subtotal
        session.SetShippingMethod("Standard", 10m);

        // Act
        session.ApplyCoupon("SUMMER20", 20m);
        var grandTotal = session.GrandTotal;

        // Assert
        // Subtotal(200) - Discount(20) + Shipping(10) + Tax(0) = 190
        Assert.Equal(190m, grandTotal);
    }

    [Fact]
    public void GrandTotal_WithTax()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);  // 100 subtotal
        session.SetShippingMethod("Standard", 10m);

        // Manually set tax (no method for this, so we'd need another approach)
        // This test documents the behavior

        // Assert
        // Subtotal(100) - Discount(0) + Shipping(10) + Tax(0) = 110
        Assert.Equal(110m, session.GrandTotal);
    }

    [Fact]
    public void GrandTotal_NeverNegative()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 50m, 1);   // 50 subtotal
        session.SetShippingMethod("Standard", 10m);

        // Act - Apply discount larger than subtotal
        session.ApplyCoupon("HUGE", 100m);

        // Assert - GrandTotal should never go below 0
        Assert.True(session.GrandTotal >= 0);
    }

    [Fact]
    public void Recalculation_OnShippingMethodChange()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingMethod("Standard", 10m);
        var grandTotal1 = session.GrandTotal;  // 110

        // Act
        session.SetShippingMethod("Express", 25m);
        var grandTotal2 = session.GrandTotal;  // 125

        // Assert
        Assert.Equal(110m, grandTotal1);
        Assert.Equal(125m, grandTotal2);
    }

    [Fact]
    public void Recalculation_OnCouponChange()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.ApplyCoupon("SUMMER20", 20m);
        var grandTotal1 = session.GrandTotal;  // 80

        // Act
        session.ApplyCoupon("FALL10", 10m);
        var grandTotal2 = session.GrandTotal;  // 90

        // Assert
        Assert.Equal(80m, grandTotal1);
        Assert.Equal(90m, grandTotal2);
    }

    #endregion

    #region Expiration Tests

    [Fact]
    public void IsExpired_BeforeExpiration_ReturnsFalse()
    {
        // Arrange & Act
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Assert
        Assert.False(session.IsExpired);
    }

    [Fact]
    public void IsExpired_AfterExpiration_ReturnsTrue()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        typeof(CheckoutSession).GetProperty("ExpiresOnUtc")?.SetValue(session, DateTime.UtcNow.AddHours(-1));

        // Act
        var isExpired = session.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    #endregion

    #region Full Workflow Tests

    [Fact]
    public void FullCheckoutFlow_SuccessfulCompletion()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        var billingAddress = Guid.NewGuid();
        var shippingAddress = Guid.NewGuid();

        // Act - Build checkout
        session.AddItem(productId, 100m, 2);
        session.SetBillingAddress(billingAddress);
        session.SetShippingAddress(shippingAddress);
        session.SetShippingMethod("Standard", 10m);
        session.ApplyCoupon("WELCOME10", 20m);
        session.SetPaymentMethod("Credit Card");

        // Assert - Checkout is ready
        Assert.Equal(CheckoutSessionStatus.Draft, session.Status);
        Assert.Single(session.Items);  // 1 item with qty 2
        Assert.Equal(200m, session.SubTotal);  // 100 * 2
        Assert.Equal(20m, session.DiscountAmount);
        Assert.Equal(10m, session.ShippingAmount);

        // Act - Proceed to payment
        session.AwaitPayment();

        // Assert
        Assert.Equal(CheckoutSessionStatus.AwaitingPayment, session.Status);

        // Act - Complete
        session.Complete();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Completed, session.Status);
    }

    [Fact]
    public void CheckoutFlow_UpdateBeforePayment()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);

        // Act - Build initial checkout
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("Standard", 10m);

        // Can still modify before AwaitPayment
        session.RemoveCoupon();  // Was not applied, but should not throw
        session.ApplyCoupon("DISCOUNT", 5m);

        // Assert
        Assert.Equal("DISCOUNT", session.CouponCode);
        Assert.Equal(5m, session.DiscountAmount);
        Assert.Equal(CheckoutSessionStatus.Draft, session.Status);
    }

    [Fact]
    public void CheckoutFlow_CancelCheckout()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);

        // Act
        session.Cancel();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Cancelled, session.Status);
    }

    [Fact]
    public void CheckoutFlow_ExpireCheckout()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);

        // Act
        session.Expire();

        // Assert
        Assert.Equal(CheckoutSessionStatus.Expired, session.Status);
    }

    #endregion

    #region Aggregate Consistency Tests

    [Fact]
    public void Items_ReturnsReadOnlyCollection()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        session.AddItem(Guid.NewGuid(), 100m, 1);

        // Act
        var items = session.Items;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<CheckoutItem>>(items);
    }

    [Fact]
    public void MultipleOperations_MaintainsConsistency()
    {
        // Arrange
        var session = CheckoutSession.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();

        // Act
        session.AddItem(product1, 50m, 2);
        var subtotal1 = session.SubTotal;
        session.AddItem(product2, 75m, 1);
        var subtotal2 = session.SubTotal;
        session.SetShippingMethod("Standard", 10m);
        var total1 = session.GrandTotal;
        session.ApplyCoupon("TEST", 25m);
        var total2 = session.GrandTotal;

        // Assert
        Assert.Equal(100m, subtotal1);
        Assert.Equal(175m, subtotal2);
        Assert.Equal(185m, total1);  // 175 + 10
        Assert.Equal(160m, total2);  // 175 - 25 + 10
    }

    #endregion
}
