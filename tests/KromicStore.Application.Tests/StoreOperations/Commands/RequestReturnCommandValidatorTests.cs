using FluentAssertions;
using KromicStore.Application.StoreOperations.Commands.RequestReturn;
using Xunit;

namespace KromicStore.Application.Tests.StoreOperations.Commands;

public class RequestReturnCommandValidatorTests
{
    private readonly RequestReturnValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidData_SuccessfulValidation()
    {
        // Arrange
        var command = new RequestReturnCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Reason = "Defective product",
            ItemCount = 2,
            ReturnAmount = 99.98m
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_WithEmptyOrderId_FailsValidation()
    {
        // Arrange
        var command = new RequestReturnCommand
        {
            OrderId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            Reason = "Defective",
            ItemCount = 1,
            ReturnAmount = 50m
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithZeroItemCount_FailsValidation()
    {
        // Arrange
        var command = new RequestReturnCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Reason = "Defective",
            ItemCount = 0,
            ReturnAmount = 50m
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithZeroReturnAmount_FailsValidation()
    {
        // Arrange
        var command = new RequestReturnCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Reason = "Defective",
            ItemCount = 1,
            ReturnAmount = 0m
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
}
