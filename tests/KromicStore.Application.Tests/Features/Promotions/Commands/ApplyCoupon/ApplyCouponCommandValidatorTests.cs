using FluentValidation.TestHelper;
using KromicStore.Application.Features.Promotions.Commands.ApplyCoupon;
using Xunit;

namespace KromicStore.Application.Tests.Features.Promotions.Commands.ApplyCoupon;

public sealed class ApplyCouponCommandValidatorTests
{
    private readonly ApplyCouponCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.NewGuid(),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldHaveError()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = string.Empty,
            OrderId = Guid.NewGuid(),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CouponCode)
            .WithErrorMessage("Coupon code is required");
    }

    [Fact]
    public void Validate_WithExcessivelyLongCode_ShouldHaveError()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = new string('a', 101),
            OrderId = Guid.NewGuid(),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CouponCode)
            .WithErrorMessage("Coupon code cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveError()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.Empty,
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("Order ID is required");
    }

    [Fact]
    public void Validate_WithZeroOrderAmount_ShouldHaveError()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.NewGuid(),
            OrderAmount = 0m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderAmount)
            .WithErrorMessage("Order amount must be greater than 0");
    }

    [Fact]
    public void Validate_WithNegativeOrderAmount_ShouldHaveError()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.NewGuid(),
            OrderAmount = -100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OrderAmount)
            .WithErrorMessage("Order amount must be greater than 0");
    }

    [Fact]
    public void Validate_WithMaxLengthCode_ShouldNotHaveErrors()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = new string('a', 100),
            OrderId = Guid.NewGuid(),
            OrderAmount = 100m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSmallOrderAmount_ShouldNotHaveErrors()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.NewGuid(),
            OrderAmount = 0.01m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithLargeOrderAmount_ShouldNotHaveErrors()
    {
        var command = new ApplyCouponCommand
        {
            CouponCode = "SUMMER2024",
            OrderId = Guid.NewGuid(),
            OrderAmount = 999999.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
