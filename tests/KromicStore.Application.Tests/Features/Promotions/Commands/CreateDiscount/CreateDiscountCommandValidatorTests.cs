using FluentValidation.TestHelper;
using KromicStore.Application.Features.Promotions.Commands.CreateDiscount;
using KromicStore.Domain.Promotions.Entities;
using Xunit;

namespace KromicStore.Application.Tests.Features.Promotions.Commands.CreateDiscount;

public class CreateDiscountCommandValidatorTests
{
    private readonly CreateDiscountCommandValidator _validator = new();
    private readonly DateTime _from = DateTime.UtcNow;
    private readonly DateTime _to;
    
    public CreateDiscountCommandValidatorTests()
    {
        _to = _from.AddDays(30);
    }
    
    [Fact]
    public void Validate_WithValidFixedAmountDiscount_ShouldPass()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Save $10",
            Description = "Save 10 dollars",
            Type = DiscountType.FixedAmount,
            FixedAmount = 10m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithValidPercentageDiscount_ShouldPass()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Save 20%",
            Type = DiscountType.PercentageAmount,
            PercentageAmount = 0.20m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "",
            Type = DiscountType.FixedAmount,
            FixedAmount = 10m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = new string('A', 201),
            Type = DiscountType.FixedAmount,
            FixedAmount = 10m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Validate_WithFixedAmountZero_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Discount",
            Type = DiscountType.FixedAmount,
            FixedAmount = 0,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FixedAmount);
    }
    
    [Fact]
    public void Validate_WithPercentageGreaterThanOne_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Discount",
            Type = DiscountType.PercentageAmount,
            PercentageAmount = 1.5m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PercentageAmount);
    }
    
    [Fact]
    public void Validate_WithValidToBeforeValidFrom_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Discount",
            Type = DiscountType.FixedAmount,
            FixedAmount = 10m,
            ValidFromUtc = _to,
            ValidToUtc = _from
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValidToUtc);
    }
    
    [Fact]
    public void Validate_WithPercentageAndMaxDiscount_ShouldPass()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Discount",
            Type = DiscountType.PercentageAmount,
            PercentageAmount = 0.50m,
            MaxDiscountAmount = 100m,
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithInvalidDiscountType_ShouldFail()
    {
        // Arrange
        var command = new CreateDiscountCommand
        {
            Name = "Discount",
            Type = (DiscountType)999, // Invalid type
            ValidFromUtc = _from,
            ValidToUtc = _to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }
}
