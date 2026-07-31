using FluentAssertions;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.StoreOperations.Entities;

public class InventoryAdjustmentTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesAdjustment()
    {
        // Act
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment from warehouse",
            "admin@test.com");
        
        // Assert
        adjustment.Should().NotBeNull();
        adjustment.TenantId.Should().Be(_tenantId);
        adjustment.ProductId.Should().Be(_productId);
        adjustment.AdjustmentQuantity.Should().Be(10);
        adjustment.Reason.Should().Be(AdjustmentReason.Restock);
        adjustment.ReasonNotes.Should().Be("Received shipment from warehouse");
        adjustment.Status.Should().Be(AdjustmentStatus.Pending);
        adjustment.RequestedBy.Should().Be("admin@test.com");
    }
    
    [Theory]
    [InlineData(-5)]
    [InlineData(5)]
    public void Create_WithNonZeroQuantity_Creates(int quantity)
    {
        // Act
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            quantity,
            AdjustmentReason.Correction,
            "Correcting inventory count",
            "admin@test.com");
        
        // Assert
        adjustment.AdjustmentQuantity.Should().Be(quantity);
    }
    
    [Fact]
    public void Create_WithZeroQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InventoryAdjustment.Create(
            _tenantId,
            _productId,
            0,
            AdjustmentReason.Correction,
            "Test",
            "admin@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Adjustment quantity cannot be zero*");
    }
    
    [Fact]
    public void Create_WithEmptyProductId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InventoryAdjustment.Create(
            _tenantId,
            Guid.Empty,
            10,
            AdjustmentReason.Restock,
            "Notes",
            "admin@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Product ID is required*");
    }
    
    [Fact]
    public void Create_WithNullReasonNotes_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            null!,
            "admin@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Reason notes are required*");
    }
    
    [Fact]
    public void Approve_FromPending_ApprovesPendingAdjustment()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        var beforeApprove = DateTime.UtcNow;
        
        // Act
        adjustment.Approve("manager@test.com");
        var afterApprove = DateTime.UtcNow;
        
        // Assert
        adjustment.Status.Should().Be(AdjustmentStatus.Approved);
        adjustment.ApprovedBy.Should().Be("manager@test.com");
        adjustment.ApprovedOnUtc.Should().BeOnOrAfter(beforeApprove);
        adjustment.ApprovedOnUtc.Should().BeOnOrBefore(afterApprove);
    }
    
    [Fact]
    public void Approve_FromApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        
        // Act & Assert
        var act = () => adjustment.Approve("manager2@test.com");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot approve adjustment with status*");
    }
    
    [Fact]
    public void Approve_WithNullApprovedBy_ThrowsArgumentException()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        
        // Act & Assert
        var act = () => adjustment.Approve(null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Approved by is required*");
    }
    
    [Fact]
    public void Reject_FromPending_RejectsPendingAdjustment()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Damage,
            "Items damaged in shipment",
            "admin@test.com");
        var beforeReject = DateTime.UtcNow;
        
        // Act
        adjustment.Reject("Insufficient documentation", "manager@test.com");
        var afterReject = DateTime.UtcNow;
        
        // Assert
        adjustment.Status.Should().Be(AdjustmentStatus.Rejected);
        adjustment.RejectionReason.Should().Be("Insufficient documentation");
        adjustment.RejectedOnUtc.Should().BeOnOrAfter(beforeReject);
        adjustment.RejectedOnUtc.Should().BeOnOrBefore(afterReject);
    }
    
    [Fact]
    public void Reject_FromApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        
        // Act & Assert
        var act = () => adjustment.Reject("Wrong reason", "manager@test.com");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot reject adjustment with status*");
    }
    
    [Fact]
    public void Apply_FromApproved_AppliesPendingAdjustment()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        var beforeApply = DateTime.UtcNow;
        
        // Act
        adjustment.Apply();
        var afterApply = DateTime.UtcNow;
        
        // Assert
        adjustment.Status.Should().Be(AdjustmentStatus.Applied);
        adjustment.AppliedOnUtc.Should().BeOnOrAfter(beforeApply);
        adjustment.AppliedOnUtc.Should().BeOnOrBefore(afterApply);
    }
    
    [Fact]
    public void Apply_FromPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        
        // Act & Assert
        var act = () => adjustment.Apply();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot apply adjustment with status*");
    }
    
    [Fact]
    public void CanApprove_WhenPending_ReturnsTrue()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        
        // Act & Assert
        adjustment.CanApprove().Should().BeTrue();
    }
    
    [Fact]
    public void CanApprove_WhenApproved_ReturnsFalse()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        
        // Act & Assert
        adjustment.CanApprove().Should().BeFalse();
    }
    
    [Fact]
    public void CanReject_WhenPending_ReturnsTrue()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        
        // Act & Assert
        adjustment.CanReject().Should().BeTrue();
    }
    
    [Fact]
    public void CanReject_WhenApproved_ReturnsFalse()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        
        // Act & Assert
        adjustment.CanReject().Should().BeFalse();
    }
    
    [Fact]
    public void CanApply_WhenApproved_ReturnsTrue()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        adjustment.Approve("manager@test.com");
        
        // Act & Assert
        adjustment.CanApply().Should().BeTrue();
    }
    
    [Fact]
    public void CanApply_WhenPending_ReturnsFalse()
    {
        // Arrange
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            10,
            AdjustmentReason.Restock,
            "Received shipment",
            "admin@test.com");
        
        // Act & Assert
        adjustment.CanApply().Should().BeFalse();
    }
    
    [Theory]
    [InlineData(AdjustmentReason.Damage)]
    [InlineData(AdjustmentReason.Loss)]
    [InlineData(AdjustmentReason.Miscount)]
    [InlineData(AdjustmentReason.Restock)]
    [InlineData(AdjustmentReason.Return)]
    [InlineData(AdjustmentReason.Correction)]
    [InlineData(AdjustmentReason.Expiration)]
    public void Create_WithAllReasons_Creates(AdjustmentReason reason)
    {
        // Act
        var adjustment = InventoryAdjustment.Create(
            _tenantId,
            _productId,
            5,
            reason,
            "Test adjustment",
            "admin@test.com");
        
        // Assert
        adjustment.Reason.Should().Be(reason);
    }
}
