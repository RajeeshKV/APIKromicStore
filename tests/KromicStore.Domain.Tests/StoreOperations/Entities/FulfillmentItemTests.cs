using FluentAssertions;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.StoreOperations.Entities;

public class FulfillmentItemTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _fulfillmentId = Guid.NewGuid();
    private readonly Guid _orderLineItemId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesItem()
    {
        // Act
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            2,
            999.99m,
            "warehouse@test.com");
        
        // Assert
        item.Should().NotBeNull();
        item.TenantId.Should().Be(_tenantId);
        item.FulfillmentId.Should().Be(_fulfillmentId);
        item.OrderLineItemId.Should().Be(_orderLineItemId);
        item.ProductId.Should().Be(_productId);
        item.ProductName.Should().Be("Laptop");
        item.SKU.Should().Be("LAPTOP-001");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(999.99m);
        item.PickedQuantity.Should().Be(0);
        item.PackedQuantity.Should().Be(0);
    }
    
    [Fact]
    public void Create_NormalizeSKUToUpperCase()
    {
        // Act
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "laptop-001",
            1,
            999.99m,
            "warehouse@test.com");
        
        // Assert
        item.SKU.Should().Be("LAPTOP-001");
    }
    
    [Fact]
    public void Create_WithEmptyFulfillmentId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            Guid.Empty,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            1,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Fulfillment ID is required*");
    }
    
    [Fact]
    public void Create_WithEmptyOrderLineItemId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            Guid.Empty,
            _productId,
            "Laptop",
            "LAPTOP-001",
            1,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Order line item ID is required*");
    }
    
    [Fact]
    public void Create_WithEmptyProductId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            Guid.Empty,
            "Laptop",
            "LAPTOP-001",
            1,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Product ID is required*");
    }
    
    [Fact]
    public void Create_WithNullProductName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            null!,
            "LAPTOP-001",
            1,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Product name is required*");
    }
    
    [Fact]
    public void Create_WithNullSKU_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            null!,
            1,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*SKU is required*");
    }
    
    [Fact]
    public void Create_WithZeroQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            0,
            999.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity must be greater than zero*");
    }
    
    [Fact]
    public void Create_WithNegativeUnitPrice_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            1,
            -50m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unit price cannot be negative*");
    }
    
    [Fact]
    public void RecordPickedQuantity_WithValidQuantity_Records()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        
        // Act
        item.RecordPickedQuantity(3);
        
        // Assert
        item.PickedQuantity.Should().Be(3);
    }
    
    [Fact]
    public void RecordPickedQuantity_ExceedingTotal_ThrowsArgumentException()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        
        // Act & Assert
        var act = () => item.RecordPickedQuantity(10);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Picked quantity cannot exceed total quantity*");
    }
    
    [Fact]
    public void RecordPickedQuantity_Negative_ThrowsArgumentException()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        
        // Act & Assert
        var act = () => item.RecordPickedQuantity(-1);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Picked quantity cannot be negative*");
    }
    
    [Fact]
    public void RecordPackedQuantity_WithValidQuantity_Records()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(5);
        
        // Act
        item.RecordPackedQuantity(5);
        
        // Assert
        item.PackedQuantity.Should().Be(5);
    }
    
    [Fact]
    public void RecordPackedQuantity_ExceedingPicked_ThrowsArgumentException()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(3);
        
        // Act & Assert
        var act = () => item.RecordPackedQuantity(5);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Packed quantity cannot exceed picked quantity*");
    }
    
    [Fact]
    public void IsFullyPicked_WhenAllPicked_ReturnsTrue()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(5);
        
        // Act & Assert
        item.IsFullyPicked().Should().BeTrue();
    }
    
    [Fact]
    public void IsFullyPicked_WhenPartiallyPicked_ReturnsFalse()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(3);
        
        // Act & Assert
        item.IsFullyPicked().Should().BeFalse();
    }
    
    [Fact]
    public void IsFullyPacked_WhenAllPacked_ReturnsTrue()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(5);
        item.RecordPackedQuantity(5);
        
        // Act & Assert
        item.IsFullyPacked().Should().BeTrue();
    }
    
    [Fact]
    public void IsFullyPacked_WhenPartiallyPacked_ReturnsFalse()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            5,
            999.99m,
            "warehouse@test.com");
        item.RecordPickedQuantity(5);
        item.RecordPackedQuantity(3);
        
        // Act & Assert
        item.IsFullyPacked().Should().BeFalse();
    }
    
    [Fact]
    public void GetTotalPrice_CalculatesCorrectly()
    {
        // Arrange
        var item = FulfillmentItem.Create(
            _tenantId,
            _fulfillmentId,
            _orderLineItemId,
            _productId,
            "Laptop",
            "LAPTOP-001",
            3,
            999.99m,
            "warehouse@test.com");
        
        // Act
        var totalPrice = item.GetTotalPrice();
        
        // Assert
        totalPrice.Should().Be(2999.97m);
    }
}
