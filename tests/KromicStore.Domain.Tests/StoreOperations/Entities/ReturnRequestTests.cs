using FluentAssertions;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.StoreOperations.Entities;

public class ReturnRequestTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesReturnRequest()
    {
        // Act
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective product",
            "The product stopped working after one week",
            2,
            99.98m,
            "customer@test.com");
        
        // Assert
        returnRequest.Should().NotBeNull();
        returnRequest.TenantId.Should().Be(_tenantId);
        returnRequest.OrderId.Should().Be(_orderId);
        returnRequest.CustomerId.Should().Be(_customerId);
        returnRequest.Status.Should().Be(ReturnStatus.Requested);
        returnRequest.Reason.Should().Be("Defective product");
        returnRequest.CustomerNotes.Should().Be("The product stopped working after one week");
        returnRequest.ItemCount.Should().Be(2);
        returnRequest.ReturnAmount.Should().Be(99.98m);
    }
    
    [Fact]
    public void Create_WithEmptyOrderId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnRequest.Create(
            _tenantId,
            Guid.Empty,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Order ID is required*");
    }
    
    [Fact]
    public void Create_WithEmptyCustomerId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnRequest.Create(
            _tenantId,
            _orderId,
            Guid.Empty,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer ID is required*");
    }
    
    [Fact]
    public void Create_WithZeroItemCount_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            0,
            50m,
            "customer@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Item count must be greater than zero*");
    }
    
    [Fact]
    public void Create_WithZeroReturnAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            0m,
            "customer@test.com");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Return amount must be greater than zero*");
    }
    
    [Fact]
    public void Approve_FromRequested_ApprovesReturn()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        var beforeApprove = DateTime.UtcNow;
        
        // Act
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        var afterApprove = DateTime.UtcNow;
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.Approved);
        returnRequest.ReturnShippingLabel.Should().Be("LABEL-123456");
        returnRequest.ApprovedBy.Should().Be("manager@test.com");
        returnRequest.ApprovedOnUtc.Should().BeOnOrAfter(beforeApprove);
        returnRequest.ApprovedOnUtc.Should().BeOnOrBefore(afterApprove);
    }
    
    [Fact]
    public void Approve_WithoutShippingLabel_ThrowsArgumentException()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        
        // Act & Assert
        var act = () => returnRequest.Approve(null!, "manager@test.com");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Return shipping label is required*");
    }
    
    [Fact]
    public void Reject_FromRequested_RejectsReturn()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        var beforeReject = DateTime.UtcNow;
        
        // Act
        returnRequest.Reject("Outside return window", "manager@test.com");
        var afterReject = DateTime.UtcNow;
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.Rejected);
        returnRequest.RejectionReason.Should().Be("Outside return window");
        returnRequest.RejectedOnUtc.Should().BeOnOrAfter(beforeReject);
        returnRequest.RejectedOnUtc.Should().BeOnOrBefore(afterReject);
    }
    
    [Fact]
    public void Reject_FromApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        
        // Act & Assert
        var act = () => returnRequest.Reject("Wrong reason", "manager@test.com");
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void RecordReturnShipment_FromApproved_RecordsTracking()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        var beforeRecord = DateTime.UtcNow;
        
        // Act
        returnRequest.RecordReturnShipment("1Z999AA10123456784");
        var afterRecord = DateTime.UtcNow;
        
        // Assert
        returnRequest.ReturnTrackingNumber.Should().Be("1Z999AA10123456784");
        returnRequest.ReturnShippedOnUtc.Should().BeOnOrAfter(beforeRecord);
        returnRequest.ReturnShippedOnUtc.Should().BeOnOrBefore(afterRecord);
    }
    
    [Fact]
    public void ReceiveReturnItems_FromApproved_ReceivesItems()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        var beforeReceive = DateTime.UtcNow;
        
        // Act
        returnRequest.ReceiveReturnItems("Items received in good condition");
        var afterReceive = DateTime.UtcNow;
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.Received);
        returnRequest.ReceivedOnUtc.Should().BeOnOrAfter(beforeReceive);
        returnRequest.ReceivedOnUtc.Should().BeOnOrBefore(afterReceive);
        returnRequest.ReceivedNotes.Should().Be("Items received in good condition");
    }
    
    [Fact]
    public void MarkAsInInspection_FromReceived_MarksInInspection()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        returnRequest.ReceiveReturnItems();
        
        // Act
        returnRequest.MarkAsInInspection();
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.InInspection);
    }
    
    [Fact]
    public void Complete_FromInInspection_Completes()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        returnRequest.ReceiveReturnItems();
        returnRequest.MarkAsInInspection();
        var beforeComplete = DateTime.UtcNow;
        
        // Act
        returnRequest.Complete();
        var afterComplete = DateTime.UtcNow;
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.Completed);
        returnRequest.CompletedOnUtc.Should().BeOnOrAfter(beforeComplete);
        returnRequest.CompletedOnUtc.Should().BeOnOrBefore(afterComplete);
    }
    
    [Fact]
    public void Cancel_FromRequested_Cancels()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        
        // Act
        returnRequest.Cancel();
        
        // Assert
        returnRequest.Status.Should().Be(ReturnStatus.Cancelled);
    }
    
    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        returnRequest.ReceiveReturnItems();
        returnRequest.MarkAsInInspection();
        returnRequest.Complete();
        
        // Act & Assert
        var act = () => returnRequest.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void CanApprove_WhenRequested_ReturnsTrue()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        
        // Act & Assert
        returnRequest.CanApprove().Should().BeTrue();
    }
    
    [Fact]
    public void CanApprove_WhenApproved_ReturnsFalse()
    {
        // Arrange
        var returnRequest = ReturnRequest.Create(
            _tenantId,
            _orderId,
            _customerId,
            "Defective",
            null,
            1,
            50m,
            "customer@test.com");
        returnRequest.Approve("LABEL-123456", "manager@test.com");
        
        // Act & Assert
        returnRequest.CanApprove().Should().BeFalse();
    }
}
