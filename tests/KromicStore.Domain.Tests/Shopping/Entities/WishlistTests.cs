using Xunit;
using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Domain.Tests.Shopping.Entities;

/// <summary>
/// Domain tests for Wishlist aggregate root.
/// Tests verify business rules, duplicate prevention, and aggregate consistency.
/// </summary>
public class WishlistTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    #region Wishlist Creation Tests

    [Fact]
    public void Create_WithValidData_CreatesWishlist()
    {
        // Arrange & Act
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Assert
        Assert.NotNull(wishlist);
        Assert.Equal(_customerId, wishlist.CustomerId);
        Assert.Equal(_tenantId, wishlist.TenantId);
        Assert.True(wishlist.IsEmpty);
        Assert.Empty(wishlist.Items);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Wishlist.Create(_tenantId, Guid.Empty));
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsException()
    {
        // Act & Assert - TenantEntity base class validates this
        var ex = Assert.Throws<ArgumentException>(() => Wishlist.Create(Guid.Empty, _customerId));
        Assert.Contains("TenantId", ex.Message);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Arrange & Act
        var wishlist1 = Wishlist.Create(_tenantId, _customerId);
        var wishlist2 = Wishlist.Create(_tenantId, _customerId);

        // Assert
        Assert.NotEqual(wishlist1.Id, wishlist2.Id);
    }

    [Fact]
    public void Create_StartsEmpty()
    {
        // Arrange & Act
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Assert
        Assert.Empty(wishlist.Items);
        Assert.Equal(0, wishlist.GetItemsCount());
        Assert.True(wishlist.IsEmpty);
    }

    #endregion

    #region Add Item Tests

    [Fact]
    public void AddItem_WithValidProductId_AddsItem()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();

        // Act
        wishlist.AddItem(productId);

        // Assert
        Assert.Single(wishlist.Items);
        Assert.Equal(1, wishlist.GetItemsCount());
        Assert.False(wishlist.IsEmpty);
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wishlist.AddItem(Guid.Empty));
    }

    [Fact]
    public void AddItem_MultipleUniqueProducts_AddsAll()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        var product3 = Guid.NewGuid();

        // Act
        wishlist.AddItem(product1);
        wishlist.AddItem(product2);
        wishlist.AddItem(product3);

        // Assert
        Assert.Equal(3, wishlist.GetItemsCount());
        Assert.Equal(3, wishlist.Items.Count);
    }

    [Fact]
    public void AddItem_DuplicateProduct_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => wishlist.AddItem(productId));
        Assert.Contains("already in the wishlist", ex.Message);
    }

    [Fact]
    public void AddItem_ToDeletedWishlist_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.SoftDelete(DateTime.UtcNow, "system");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.AddItem(Guid.NewGuid()));
    }

    [Fact]
    public void AddItem_RecordsAddedTime()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        var beforeAdd = DateTime.UtcNow;

        // Act
        wishlist.AddItem(productId);

        // Assert
        var addedTime = wishlist.Items.First().AddedOnUtc;
        Assert.True(addedTime >= beforeAdd.AddSeconds(-1));
        Assert.True(addedTime <= DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public void RemoveItem_WithExistentItem_RemovesItem()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);

        // Act
        wishlist.RemoveItem(productId);

        // Assert
        Assert.Empty(wishlist.Items);
        Assert.True(wishlist.IsEmpty);
    }

    [Fact]
    public void RemoveItem_WithNonExistentItem_DoesNotThrow()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act & Assert - Should not throw
        wishlist.RemoveItem(Guid.NewGuid());
        Assert.Empty(wishlist.Items);
    }

    [Fact]
    public void RemoveItem_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wishlist.RemoveItem(Guid.Empty));
    }

    [Fact]
    public void RemoveItem_PartialRemoval_KeepsOtherItems()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        var product3 = Guid.NewGuid();
        wishlist.AddItem(product1);
        wishlist.AddItem(product2);
        wishlist.AddItem(product3);

        // Act
        wishlist.RemoveItem(product2);

        // Assert
        Assert.Equal(2, wishlist.GetItemsCount());
        Assert.True(wishlist.ContainsProduct(product1));
        Assert.False(wishlist.ContainsProduct(product2));
        Assert.True(wishlist.ContainsProduct(product3));
    }

    [Fact]
    public void RemoveItem_FromDeletedWishlist_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);
        wishlist.SoftDelete(DateTime.UtcNow, "system");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.RemoveItem(productId));
    }

    #endregion

    #region Contains Product Tests

    [Fact]
    public void ContainsProduct_WithExistentProduct_ReturnsTrue()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);

        // Act
        var contains = wishlist.ContainsProduct(productId);

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void ContainsProduct_WithNonExistentProduct_ReturnsFalse()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act
        var contains = wishlist.ContainsProduct(Guid.NewGuid());

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void ContainsProduct_AfterRemoval_ReturnsFalse()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);
        wishlist.RemoveItem(productId);

        // Act
        var contains = wishlist.ContainsProduct(productId);

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void ContainsProduct_EmptyProductId_ReturnsFalse()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act
        var contains = wishlist.ContainsProduct(Guid.Empty);

        // Assert
        Assert.False(contains);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithItems_RemovesAll()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());

        // Act
        wishlist.Clear();

        // Assert
        Assert.Empty(wishlist.Items);
        Assert.True(wishlist.IsEmpty);
        Assert.Equal(0, wishlist.GetItemsCount());
    }

    [Fact]
    public void Clear_EmptyWishlist_RemainsEmpty()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act
        wishlist.Clear();

        // Assert
        Assert.Empty(wishlist.Items);
        Assert.True(wishlist.IsEmpty);
    }

    [Fact]
    public void Clear_DeletedWishlist_ThrowsException()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.SoftDelete(DateTime.UtcNow, "system");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.Clear());
    }

    #endregion

    #region Items Count Tests

    [Fact]
    public void GetItemsCount_ReturnsCorrectCount()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());

        // Act
        var count = wishlist.GetItemsCount();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void GetItemsCount_EmptyWishlist_ReturnsZero()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Act
        var count = wishlist.GetItemsCount();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void GetItemsCount_AfterRemoval_ReturnsCorrectCount()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        wishlist.AddItem(product1);
        wishlist.AddItem(product2);

        // Act
        wishlist.RemoveItem(product1);
        var count = wishlist.GetItemsCount();

        // Assert
        Assert.Equal(1, count);
    }

    #endregion

    #region IsEmpty Tests

    [Fact]
    public void IsEmpty_NewWishlist_ReturnsTrue()
    {
        // Arrange & Act
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Assert
        Assert.True(wishlist.IsEmpty);
    }

    [Fact]
    public void IsEmpty_WithItems_ReturnsFalse()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());

        // Act & Assert
        Assert.False(wishlist.IsEmpty);
    }

    [Fact]
    public void IsEmpty_AfterClear_ReturnsTrue()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());

        // Act
        wishlist.Clear();

        // Assert
        Assert.True(wishlist.IsEmpty);
    }

    [Fact]
    public void IsEmpty_AfterRemovingAllItems_ReturnsTrue()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        wishlist.AddItem(product1);
        wishlist.AddItem(product2);

        // Act
        wishlist.RemoveItem(product1);
        wishlist.RemoveItem(product2);

        // Assert
        Assert.True(wishlist.IsEmpty);
    }

    #endregion

    #region Aggregate Consistency Tests

    [Fact]
    public void AddThenRemove_MaintainsConsistency()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();

        // Act
        wishlist.AddItem(product1);
        wishlist.AddItem(product2);
        wishlist.RemoveItem(product1);
        wishlist.AddItem(product1);

        // Assert
        Assert.Equal(2, wishlist.GetItemsCount());
        Assert.True(wishlist.ContainsProduct(product1));
        Assert.True(wishlist.ContainsProduct(product2));
    }

    [Fact]
    public void Items_ReturnsReadOnlyCollection()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());

        // Act
        var items = wishlist.Items;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<WishlistItem>>(items);
    }

    [Fact]
    public void MultipleOperations_MaintainsState()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var products = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        // Act - Add all
        foreach (var product in products)
        {
            wishlist.AddItem(product);
        }

        // Assert after add
        Assert.Equal(5, wishlist.GetItemsCount());

        // Act - Remove middle items
        wishlist.RemoveItem(products[1]);
        wishlist.RemoveItem(products[3]);

        // Assert after remove
        Assert.Equal(3, wishlist.GetItemsCount());
        Assert.True(wishlist.ContainsProduct(products[0]));
        Assert.False(wishlist.ContainsProduct(products[1]));
        Assert.True(wishlist.ContainsProduct(products[2]));
        Assert.False(wishlist.ContainsProduct(products[3]));
        Assert.True(wishlist.ContainsProduct(products[4]));
    }

    [Fact]
    public void DuplicatePrevention_IsEnforced()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var productId = Guid.NewGuid();

        // Act & Assert
        wishlist.AddItem(productId);
        
        for (int i = 0; i < 3; i++)
        {
            Assert.Throws<InvalidOperationException>(() => wishlist.AddItem(productId));
        }

        // Verify item count didn't increase
        Assert.Single(wishlist.Items);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void AddLargeNumberOfItems_Succeeds()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var products = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

        // Act
        foreach (var product in products)
        {
            wishlist.AddItem(product);
        }

        // Assert
        Assert.Equal(100, wishlist.GetItemsCount());
    }

    [Fact]
    public void ItemsCollection_ReturnsNewReadOnlyList()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.AddItem(Guid.NewGuid());

        // Act
        var items1 = wishlist.Items;
        var items2 = wishlist.Items;

        // Assert - Different instances but same content
        Assert.NotSame(items1, items2);
        Assert.Equal(items1.Count, items2.Count);
        Assert.Equal(2, items1.Count);
    }

    [Fact]
    public void RemoveNonExistentItem_DoesNotAffectExisting()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        wishlist.AddItem(product1);

        // Act
        wishlist.RemoveItem(product2);

        // Assert
        Assert.Single(wishlist.Items);
        Assert.True(wishlist.ContainsProduct(product1));
    }

    [Fact]
    public void TenantIsolation_MultipleWishlists()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var customer1 = Guid.NewGuid();
        var customer2 = Guid.NewGuid();

        // Act
        var wishlist1 = Wishlist.Create(tenant1, customer1);
        var wishlist2 = Wishlist.Create(tenant2, customer2);

        // Assert
        Assert.Equal(tenant1, wishlist1.TenantId);
        Assert.Equal(tenant2, wishlist2.TenantId);
        Assert.NotEqual(wishlist1.TenantId, wishlist2.TenantId);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void NewWishlist_IsNotDeleted()
    {
        // Arrange & Act
        var wishlist = Wishlist.Create(_tenantId, _customerId);

        // Assert
        Assert.False(wishlist.IsDeleted);
    }

    [Fact]
    public void CannotAddToDeletedWishlist()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.SoftDelete(DateTime.UtcNow, "system");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.AddItem(Guid.NewGuid()));
    }

    [Fact]
    public void CannotRemoveFromDeletedWishlist()
    {
        // Arrange
        var wishlist = Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(Guid.NewGuid());
        wishlist.SoftDelete(DateTime.UtcNow, "system");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.RemoveItem(Guid.NewGuid()));
    }

    #endregion
}
