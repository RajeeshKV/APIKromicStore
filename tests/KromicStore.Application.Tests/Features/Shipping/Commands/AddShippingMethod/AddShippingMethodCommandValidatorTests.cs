using FluentValidation.TestHelper;
using KromicStore.Application.Features.Shipping.Commands.AddShippingMethod;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shipping.Commands.AddShippingMethod;

public sealed class AddShippingMethodCommandValidatorTests
{
    private readonly AddShippingMethodCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express Shipping",
            Code = "EXPRESS",
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyZoneId_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.Empty,
            Name = "Express",
            Code = "EXPRESS",
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ShippingZoneId)
            .WithErrorMessage("Shipping zone is required");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = string.Empty,
            Code = "EXPRESS",
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Method name is required");
    }

    [Fact]
    public void Validate_WithExcessivelyLongName_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = new string('a', 101),
            Code = "EXPRESS",
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Method name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express",
            Code = string.Empty,
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Method code is required");
    }

    [Fact]
    public void Validate_WithExcessivelyLongCode_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express",
            Code = new string('a', 51),
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Method code cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_WithZeroMinimumDays_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express",
            Code = "EXPRESS",
            EstimatedDaysMin = 0,
            EstimatedDaysMax = 3
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EstimatedDaysMin)
            .WithErrorMessage("Minimum estimated days must be greater than 0");
    }

    [Fact]
    public void Validate_WithMaxLessThanMin_ShouldHaveError()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express",
            Code = "EXPRESS",
            EstimatedDaysMin = 5,
            EstimatedDaysMax = 2
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EstimatedDaysMax)
            .WithErrorMessage("Maximum estimated days must be >= minimum");
    }

    [Fact]
    public void Validate_WithMinEqualToMax_ShouldNotHaveErrors()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Standard",
            Code = "STANDARD",
            EstimatedDaysMin = 5,
            EstimatedDaysMax = 5
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOptionalDescription_ShouldNotHaveErrors()
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = Guid.NewGuid(),
            Name = "Express",
            Code = "EXPRESS",
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 3,
            Description = "Fast delivery option"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
