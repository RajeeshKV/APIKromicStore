using FluentValidation.TestHelper;
using KromicStore.Application.Features.Taxes.Commands.CreateTaxRule;
using Xunit;

namespace KromicStore.Application.Tests.Features.Taxes.Commands.CreateTaxRule;

public class CreateTaxRuleCommandValidatorTests
{
    private readonly CreateTaxRuleCommandValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = 0.15m,
            Description = "Electronics tax"
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyTaxRegionId_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.Empty,
            ProductCategory = "Electronics",
            TaxRate = 0.15m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxRegionId);
    }
    
    [Fact]
    public void Validate_WithEmptyCategory_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "",
            TaxRate = 0.15m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductCategory);
    }
    
    [Fact]
    public void Validate_WithCategoryTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = new string('A', 201),
            TaxRate = 0.15m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductCategory);
    }
    
    [Fact]
    public void Validate_WithNegativeTaxRate_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = -0.1m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxRate);
    }
    
    [Fact]
    public void Validate_WithTaxRateGreaterThanOne_ShouldFail()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = 1.5m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxRate);
    }
    
    [Fact]
    public void Validate_WithValidDateRange_ShouldPass()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = 0.15m,
            EffectiveFromUtc = from,
            EffectiveToUtc = to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithInvalidDateRange_ShouldFail()
    {
        // Arrange
        var to = DateTime.UtcNow;
        var from = to.AddDays(1);
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = 0.15m,
            EffectiveFromUtc = from,
            EffectiveToUtc = to
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EffectiveToUtc);
    }
    
    [Fact]
    public void Validate_WithZeroTaxRate_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Clothing",
            TaxRate = 0m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithMaxTaxRate_ShouldPass()
    {
        // Arrange
        var command = new CreateTaxRuleCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            TaxRate = 1.0m
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
