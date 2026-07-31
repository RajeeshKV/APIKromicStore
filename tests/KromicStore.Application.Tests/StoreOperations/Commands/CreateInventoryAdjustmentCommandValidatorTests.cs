using FluentAssertions;
using KromicStore.Application.StoreOperations.Commands.CreateInventoryAdjustment;
using KromicStore.Domain.StoreOperations.Entities;
using Xunit;

namespace KromicStore.Application.Tests.StoreOperations.Commands;

public class CreateInventoryAdjustmentCommandValidatorTests
{
    private readonly CreateInventoryAdjustmentValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidData_SuccessfulValidation()
    {
        // Arrange
        var command = new CreateInventoryAdjustmentCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Reason = AdjustmentReason.Restock,
            ReasonNotes = "Received shipment from warehouse"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_WithEmptyProductId_FailsValidation()
    {
        // Arrange
        var command = new CreateInventoryAdjustmentCommand
        {
            ProductId = Guid.Empty,
            Quantity = 10,
            Reason = AdjustmentReason.Restock,
            ReasonNotes = "Received shipment from warehouse"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithZeroQuantity_FailsValidation()
    {
        // Arrange
        var command = new CreateInventoryAdjustmentCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0,
            Reason = AdjustmentReason.Restock,
            ReasonNotes = "Received shipment from warehouse"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithNullReasonNotes_FailsValidation()
    {
        // Arrange
        var command = new CreateInventoryAdjustmentCommand
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            Reason = AdjustmentReason.Restock,
            ReasonNotes = null!
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
}
