using Xunit;
using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Domain.Tests.Shopping.Entities;

/// <summary>
/// Domain tests for Cart aggregate root.
/// Tests verify business rules, invariants, and domain behavior.
/// </summary>
public class CartTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private const string SessionId = "guest-session-123";
    private const string Currency = "USD";

    #region Cart Creation Tests

    [Fact]
    public void CreateForCustomer_WithValidData_CreatesCustomerCart()
    {
        // Arrange & Act
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Assert
        Assert.NotNull(cart);
        Assert.Equal(_customerId, cart.CustomerId);
        Assert.Null(cart.AnonymousSessionId);
        Assert.Equal(Currency, cart.Currency);
        Assert.Equal(_tenantId, cart.TenantId);
        Assert.True(cart.IsEmpty);
    }

    [Fact]
    public void CreateForCustomer_WithEmptyCustomerId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForCustomer(_tenantId, Guid.Empty, Currency));
    }

    [Fact]
    public void CreateForCustomer_WithInvalidCurrency_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForCustomer(_tenantId, _customerId, "INVALID"));
    }

    [Fact]
    public void CreateForCustomer_WithNullCurrency_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForCustomer(_tenantId, _customerId, null!));
    }

    [Fact]
    public void CreateForGuest_WithValidSessionId_CreatesGuestCart()
    {
        // Arrange & Act
        var cart = Cart.CreateForGuest(_tenantId, SessionId, Currency);

        // Assert
        Assert.NotNull(cart);
        Assert.Null(cart.CustomerId);
        Assert.Equal(SessionId, cart.AnonymousSessionId);
        Assert.Equal(Currency, cart.Currency);
        Assert.Equal(_tenantId, cart.TenantId);
        Assert.True(cart.IsEmpty);
    }

    [Fact]
    public void CreateForGuest_WithEmptySessionId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForGuest(_tenantId, "", Currency));
    }

    [Fact]
    public void CreateForGuest_WithNullSessionId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForGuest(_tenantId, null!, Currency));
    }

    [Fact]
    public void CreateForGuest_WithWhitespaceSessionId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.CreateForGuest(_tenantId, "   ", Currency));
    }

    [Fact]
    public void CreateForCustomer_HasCorrectExpiration()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Assert
        var expectedExpiration = beforeCreation.AddDays(30);
        Assert.True(cart.ExpiresOnUtc >= expectedExpiration.AddSeconds(-1));
        Assert.True(cart.ExpiresOnUtc <= expectedExpiration.AddSeconds(1));
    }

    [Fact]
    public void CreateForGuest_HasShorterExpiration()
    {
        // Arrange
        var customerCart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var guestCart = Cart.CreateForGuest(_tenantId, SessionId, Currency);

        // Act & Assert
        Assert.True(guestCart.ExpiresOnUtc < customerCart.ExpiresOnUtc);
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public void AddItem_WithValidData_AddsItemToCart()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        const decimal price = 99.99m;
        const int quantity = 2;

        // Act
        cart.AddItem(productId, price, quantity);

        // Assert
        Assert.False(cart.IsEmpty);
        Assert.Single(cart.Items);
        Assert.Equal(quantity, cart.GetItemsCount());
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(Guid.Empty, 10m, 1));
    }

    [Fact]
    public void AddItem_WithNegativePrice_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(Guid.NewGuid(), -5m, 1));
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(Guid.NewGuid(), 10m, 0));
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(Guid.NewGuid(), 10m, -1));
    }

    [Fact]
    public void AddItem_SameProductTwice_MergesQuantity()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, 50m, 2);
        cart.AddItem(productId, 50m, 3);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.GetItemsCount());
    }

    [Fact]
    public void AddItem_DifferentVariants_CreatesSeparateItems()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();

        // Act
        cart.AddItem(productId, 50m, 1, variant1);
        cart.AddItem(productId, 50m, 1, variant2);

        // Assert
        Assert.Equal(2, cart.Items.Count);
        Assert.Equal(2, cart.GetItemsCount());
    }

    [Fact]
    public void AddItem_ToDeletedCart_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.AddItem(Guid.NewGuid(), 10m, 1));
    }

    #endregion

    #region Update Item Quantity Tests

    [Fact]
    public void UpdateItemQuantity_WithValidData_UpdatesQuantity()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);

        // Act
        cart.UpdateItemQuantity(productId, 5);

        // Assert
        Assert.Equal(5, cart.GetItemsCount());
    }

    [Fact]
    public void UpdateItemQuantity_WithZero_RemovesItem()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);

        // Act
        cart.UpdateItemQuantity(productId, 0);

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentItem_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            cart.UpdateItemQuantity(Guid.NewGuid(), 5));
    }

    [Fact]
    public void UpdateItemQuantity_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            cart.UpdateItemQuantity(productId, -1));
    }

    [Fact]
    public void UpdateItemQuantity_OnDeletedCart_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        cart.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            cart.UpdateItemQuantity(productId, 5));
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public void RemoveItem_WithExistentItem_RemovesItem()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);

        // Act
        cart.RemoveItem(productId);

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_NonExistentItem_DoesNotThrow()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert (should not throw)
        cart.RemoveItem(Guid.NewGuid());
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_WithVariant_OnlyRemovesSpecificVariant()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1, variant1);
        cart.AddItem(productId, 50m, 1, variant2);

        // Act
        cart.RemoveItem(productId, variant1);

        // Assert
        Assert.Single(cart.Items);
    }

    [Fact]
    public void RemoveItem_FromDeletedCart_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        cart.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.RemoveItem(productId));
    }

    #endregion

    #region Clear Cart Tests

    [Fact]
    public void Clear_WithItems_RemovesAllItems()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        cart.AddItem(Guid.NewGuid(), 75m, 3);

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
        Assert.True(cart.IsEmpty);
    }

    [Fact]
    public void Clear_EmptyCart_RemainsEmpty()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Clear_DeletedCart_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.Delete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.Clear());
    }

    #endregion

    #region Calculations Tests

    [Fact]
    public void GetSubtotal_CalculatesCorrectly()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.AddItem(Guid.NewGuid(), 50m, 2);      // 100
        cart.AddItem(Guid.NewGuid(), 75m, 3);      // 225
        // Expected: 325

        // Act
        var subtotal = cart.GetSubtotal();

        // Assert
        Assert.Equal(325m, subtotal);
    }

    [Fact]
    public void GetSubtotal_EmptyCart_ReturnsZero()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        var subtotal = cart.GetSubtotal();

        // Assert
        Assert.Equal(0m, subtotal);
    }

    [Fact]
    public void GetItemsCount_ReturnsSumOfQuantities()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.AddItem(Guid.NewGuid(), 50m, 2);      // 2 items
        cart.AddItem(Guid.NewGuid(), 75m, 3);      // 3 items
        // Expected: 5 items total

        // Act
        var count = cart.GetItemsCount();

        // Assert
        Assert.Equal(5, count);
    }

    [Fact]
    public void GetItemsCount_EmptyCart_ReturnsZero()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        var count = cart.GetItemsCount();

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region Expiration Tests

    [Fact]
    public void IsExpired_BeforeExpirationDate_ReturnsFalse()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        var isExpired = cart.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsExpired_AfterExpirationDate_ReturnsTrue()
    {
        // Arrange - Use reflection to set expiration in the past
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        typeof(Cart).GetProperty("ExpiresOnUtc")?.SetValue(cart, DateTime.UtcNow.AddDays(-1));

        // Act
        var isExpired = cart.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    #endregion

    #region Cart Conversion Tests

    [Fact]
    public void ConvertToCustomerCart_WithValidCustomerId_ConvertsGuestCartToCustomerCart()
    {
        // Arrange
        var cart = Cart.CreateForGuest(_tenantId, SessionId, Currency);
        cart.AddItem(Guid.NewGuid(), 50m, 2);

        // Act
        cart.ConvertToCustomerCart(_customerId);

        // Assert
        Assert.Equal(_customerId, cart.CustomerId);
        Assert.Null(cart.AnonymousSessionId);
        Assert.NotEmpty(cart.Items);
    }

    [Fact]
    public void ConvertToCustomerCart_WithEmptyCustomerId_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForGuest(_tenantId, SessionId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.ConvertToCustomerCart(Guid.Empty));
    }

    [Fact]
    public void ConvertToCustomerCart_AlreadyCustomerCart_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            cart.ConvertToCustomerCart(Guid.NewGuid()));
    }

    [Fact]
    public void ConvertToCustomerCart_ExtendsExpiration()
    {
        // Arrange
        var cart = Cart.CreateForGuest(_tenantId, SessionId, Currency);
        var guestExpiration = cart.ExpiresOnUtc;

        // Act
        cart.ConvertToCustomerCart(_customerId);

        // Assert
        Assert.True(cart.ExpiresOnUtc > guestExpiration);
    }

    #endregion

    #region HasItem Tests

    [Fact]
    public void HasItem_WithExistentItem_ReturnsTrue()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1);

        // Act
        var hasItem = cart.HasItem(productId);

        // Assert
        Assert.True(hasItem);
    }

    [Fact]
    public void HasItem_WithNonExistentItem_ReturnsFalse()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        var hasItem = cart.HasItem(Guid.NewGuid());

        // Assert
        Assert.False(hasItem);
    }

    [Fact]
    public void HasItem_WithVariant_MatchesCorrectly()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1, variant1);

        // Act & Assert
        Assert.True(cart.HasItem(productId, variant1));
        Assert.False(cart.HasItem(productId, variant2));
        Assert.False(cart.HasItem(productId)); // Without variant
    }

    [Fact]
    public void HasItem_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.HasItem(Guid.Empty));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ValidCart_MarksAsDeleted()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act
        cart.Delete();

        // Assert
        Assert.True(cart.IsDeleted);
        Assert.NotNull(cart.DeletedOnUtc);
    }

    [Fact]
    public void Delete_AlreadyDeleted_RemainsDeleted()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);
        cart.Delete();
        var firstDeleteTime = cart.DeletedOnUtc;

        // Act
        cart.Delete();

        // Assert
        Assert.True(cart.IsDeleted);
        Assert.Equal(firstDeleteTime, cart.DeletedOnUtc);
    }

    #endregion

    #region Edge Cases & Boundary Tests

    [Fact]
    public void AddItem_WithMaxPrice_Succeeds()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert - Should not throw
        cart.AddItem(Guid.NewGuid(), decimal.MaxValue / 1000, 1);
    }

    [Fact]
    public void AddItem_WithLargeQuantity_Succeeds()
    {
        // Arrange
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, Currency);

        // Act & Assert - Should not throw
        cart.AddItem(Guid.NewGuid(), 50m, 999);
        Assert.Equal(999, cart.GetItemsCount());
    }

    [Fact]
    public void Currency_CaseSensitivity_StoresAsUppercase()
    {
        // Arrange & Act
        var cart = Cart.CreateForCustomer(_tenantId, _customerId, "usd");

        // Assert
        Assert.Equal("USD", cart.Currency);
    }

    #endregion
}
