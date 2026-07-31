using FluentValidation.TestHelper;
using KromicStore.Application.Features.Shipping.Commands.CalculateShippingCost;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shipping.Commands.CalculateShippingCost;

public sealed class CalculateShippingCostCommandValidatorTests
{
    private readonly CalculateShippingCostCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 5.5m,
            OrderValue = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyMethodId_ShouldHaveError()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.Empty,
            Weight = 5.5m,
            OrderValue = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ShippingMethodId)
            .WithErrorMessage("Shipping method is required");
    }

    [Fact]
    public void Validate_WithNegativeWeight_ShouldHaveError()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = -1m,
            OrderValue = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorMessage("Weight cannot be negative");
    }

    [Fact]
    public void Validate_WithZeroWeight_ShouldNotHaveErrors()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 0m,
            OrderValue = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNegativeOrderValue_ShouldHaveError()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 5.5m,
            OrderValue = -100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderValue)
            .WithErrorMessage("Order value cannot be negative");
    }

    [Fact]
    public void Validate_WithZeroOrderValue_ShouldNotHaveErrors()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 5.5m,
            OrderValue = 0m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithLargeValues_ShouldNotHaveErrors()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 999999.99m,
            OrderValue = 999999.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDecimalPrecision_ShouldNotHaveErrors()
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = Guid.NewGuid(),
            Weight = 5.5555m,
            OrderValue = 100.9999m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
