using FluentAssertions;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.StoreOperations.Entities;

public class FulfillmentTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesFulfillment()
    {
        // Act
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St, New York, NY 10001",
            9.99m,
            "warehouse@test.com");
        
        // Assert
        fulfillment.Should().NotBeNull();
        fulfillment.TenantId.Should().Be(_tenantId);
        fulfillment.OrderId.Should().Be(_orderId);
        fulfillment.ShippingAddress.Should().Be("123 Main St, New York, NY 10001");
        fulfillment.ShippingCost.Should().Be(9.99m);
        fulfillment.Status.Should().Be(FulfillmentStatus.Pending);
        fulfillment.CreatedBy.Should().Be("warehouse@test.com");
        fulfillment.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void Create_WithEmptyOrderId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Fulfillment.Create(
            _tenantId,
            Guid.Empty,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Order ID is required*");
    }
    
    [Fact]
    public void Create_WithNullShippingAddress_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Fulfillment.Create(
            _tenantId,
            _orderId,
            null!,
            9.99m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Shipping address is required*");
    }
    
    [Fact]
    public void Create_WithNegativeShippingCost_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            -5m,
            "warehouse@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Shipping cost cannot be negative*");
    }
    
    [Fact]
    public void AddItem_WithValidItem_AddsItem()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var item = FulfillmentItem.Create(
            _tenantId,
            fulfillment.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            "SKU123",
            1,
            19.99m,
            "warehouse@test.com");
        
        // Act
        fulfillment.AddItem(item);
        
        // Assert
        fulfillment.Items.Should().HaveCount(1);
        fulfillment.Items[0].Should().Be(item);
    }
    
    [Fact]
    public void AddItem_WhenNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var item = FulfillmentItem.Create(
            _tenantId,
            fulfillment.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            "SKU123",
            1,
            19.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        
        // Act & Assert
        var act = () => fulfillment.AddItem(item);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add items to fulfillment*");
    }
    
    [Fact]
    public void MarkAsProcessing_FromPending_ProcessesFulfillment()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var beforeProcessing = DateTime.UtcNow;
        
        // Act
        fulfillment.MarkAsProcessing("Started picking items");
        var afterProcessing = DateTime.UtcNow;
        
        // Assert
        fulfillment.Status.Should().Be(FulfillmentStatus.Processing);
        fulfillment.ProcessingNotes.Should().Be("Started picking items");
        fulfillment.ProcessedAtUtc.Should().BeOnOrAfter(beforeProcessing);
        fulfillment.ProcessedAtUtc.Should().BeOnOrBefore(afterProcessing);
    }
    
    [Fact]
    public void MarkAsProcessing_FromProcessing_ThrowsInvalidOperationException()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        
        // Act & Assert
        var act = () => fulfillment.MarkAsProcessing();
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void MarkAsPacked_FromProcessing_PacksFulfillment()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        var beforePacking = DateTime.UtcNow;
        
        // Act
        fulfillment.MarkAsPacked("Packed in box #123");
        var afterPacking = DateTime.UtcNow;
        
        // Assert
        fulfillment.Status.Should().Be(FulfillmentStatus.Packed);
        fulfillment.PackingNotes.Should().Be("Packed in box #123");
        fulfillment.PackedAtUtc.Should().BeOnOrAfter(beforePacking);
        fulfillment.PackedAtUtc.Should().BeOnOrBefore(afterPacking);
    }
    
    [Fact]
    public void MarkAsShipped_FromPacked_ShipsFulfillment()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        var beforeShipping = DateTime.UtcNow;
        
        // Act
        fulfillment.MarkAsShipped("1Z999AA10123456784", "UPS", "Shipped via UPS Ground");
        var afterShipping = DateTime.UtcNow;
        
        // Assert
        fulfillment.Status.Should().Be(FulfillmentStatus.Shipped);
        fulfillment.TrackingNumber.Should().Be("1Z999AA10123456784");
        fulfillment.CarrierCode.Should().Be("UPS");
        fulfillment.ShippingNotes.Should().Be("Shipped via UPS Ground");
        fulfillment.ShippedAtUtc.Should().BeOnOrAfter(beforeShipping);
        fulfillment.ShippedAtUtc.Should().BeOnOrBefore(afterShipping);
    }
    
    [Fact]
    public void MarkAsShipped_WithoutTrackingNumber_ThrowsArgumentException()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        
        // Act & Assert
        var act = () => fulfillment.MarkAsShipped(null!, "UPS");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tracking number is required*");
    }
    
    [Fact]
    public void MarkAsDelivered_FromShipped_DeliversFulfillment()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        fulfillment.MarkAsShipped("1Z999AA10123456784", "UPS");
        var beforeDelivery = DateTime.UtcNow;
        
        // Act
        fulfillment.MarkAsDelivered();
        var afterDelivery = DateTime.UtcNow;
        
        // Assert
        fulfillment.Status.Should().Be(FulfillmentStatus.Delivered);
        fulfillment.DeliveredAtUtc.Should().BeOnOrAfter(beforeDelivery);
        fulfillment.DeliveredAtUtc.Should().BeOnOrBefore(afterDelivery);
    }
    
    [Fact]
    public void Cancel_FromPending_CancelsFulfillment()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var beforeCancel = DateTime.UtcNow;
        
        // Act
        fulfillment.Cancel();
        var afterCancel = DateTime.UtcNow;
        
        // Assert
        fulfillment.Status.Should().Be(FulfillmentStatus.Cancelled);
        fulfillment.CancelledAtUtc.Should().BeOnOrAfter(beforeCancel);
        fulfillment.CancelledAtUtc.Should().BeOnOrBefore(afterCancel);
    }
    
    [Fact]
    public void Cancel_FromDelivered_ThrowsInvalidOperationException()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        fulfillment.MarkAsShipped("1Z999AA10123456784", "UPS");
        fulfillment.MarkAsDelivered();
        
        // Act & Assert
        var act = () => fulfillment.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void UpdateTrackingNumber_FromShipped_Updates()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        fulfillment.MarkAsShipped("1Z999AA10123456784", "UPS");
        
        // Act
        fulfillment.UpdateTrackingNumber("1Z999BB10987654321", "FedEx");
        
        // Assert
        fulfillment.TrackingNumber.Should().Be("1Z999BB10987654321");
        fulfillment.CarrierCode.Should().Be("FedEx");
    }
    
    [Fact]
    public void GetTotalItemCount_SumsAllItems()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var item1 = FulfillmentItem.Create(
            _tenantId,
            fulfillment.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product 1",
            "SKU1",
            5,
            19.99m,
            "warehouse@test.com");
        var item2 = FulfillmentItem.Create(
            _tenantId,
            fulfillment.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product 2",
            "SKU2",
            3,
            29.99m,
            "warehouse@test.com");
        fulfillment.AddItem(item1);
        fulfillment.AddItem(item2);
        
        // Act
        var totalCount = fulfillment.GetTotalItemCount();
        
        // Assert
        totalCount.Should().Be(8);
    }
    
    [Fact]
    public void CanProcess_WhenPendingWithItems_ReturnsTrue()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        var item = FulfillmentItem.Create(
            _tenantId,
            fulfillment.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            "SKU123",
            1,
            19.99m,
            "warehouse@test.com");
        fulfillment.AddItem(item);
        
        // Act & Assert
        fulfillment.CanProcess().Should().BeTrue();
    }
    
    [Fact]
    public void CanProcess_WhenPendingWithoutItems_ReturnsFalse()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        
        // Act & Assert
        fulfillment.CanProcess().Should().BeFalse();
    }
    
    [Fact]
    public void CanPack_WhenProcessing_ReturnsTrue()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        
        // Act & Assert
        fulfillment.CanPack().Should().BeTrue();
    }
    
    [Fact]
    public void CanShip_WhenPacked_ReturnsTrue()
    {
        // Arrange
        var fulfillment = Fulfillment.Create(
            _tenantId,
            _orderId,
            "123 Main St",
            9.99m,
            "warehouse@test.com");
        fulfillment.MarkAsProcessing();
        fulfillment.MarkAsPacked();
        
        // Act & Assert
        fulfillment.CanShip().Should().BeTrue();
    }
}
