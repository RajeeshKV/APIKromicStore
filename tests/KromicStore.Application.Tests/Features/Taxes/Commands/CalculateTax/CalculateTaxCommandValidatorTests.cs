using FluentValidation.TestHelper;
using KromicStore.Application.Features.Taxes.Commands.CalculateTax;
using Xunit;

namespace KromicStore.Application.Tests.Features.Taxes.Commands.CalculateTax;

public sealed class CalculateTaxCommandValidatorTests
{
    private readonly CalculateTaxCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRegionId_ShouldHaveError()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.Empty,
            ProductCategory = "Electronics",
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaxRegionId)
            .WithErrorMessage("Tax region is required");
    }

    [Fact]
    public void Validate_WithEmptyCategory_ShouldHaveError()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = string.Empty,
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategory)
            .WithErrorMessage("Product category is required");
    }

    [Fact]
    public void Validate_WithExcessivelyLongCategory_ShouldHaveError()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = new string('a', 101),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductCategory)
            .WithErrorMessage("Product category cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithZeroOrderAmount_ShouldHaveError()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = 0m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderAmount)
            .WithErrorMessage("Order amount must be greater than 0");
    }

    [Fact]
    public void Validate_WithNegativeOrderAmount_ShouldHaveError()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = -100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderAmount)
            .WithErrorMessage("Order amount must be greater than 0");
    }

    [Fact]
    public void Validate_WithMaxLengthCategory_ShouldNotHaveErrors()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = new string('a', 100),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSmallOrderAmount_ShouldNotHaveErrors()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = 0.01m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithLargeOrderAmount_ShouldNotHaveErrors()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = 999999.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDecimalPrecision_ShouldNotHaveErrors()
    {
        var command = new CalculateTaxCommand
        {
            TaxRegionId = Guid.NewGuid(),
            ProductCategory = "Electronics",
            OrderAmount = 100.9999m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
