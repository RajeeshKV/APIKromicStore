using FluentAssertions;
using KromicStore.Application.Features.Orders.Commands.CreateOrder;
using Xunit;

namespace KromicStore.Application.Tests.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Validator tests for CreateOrderCommand.
/// Verifies validation rules for order creation.
/// </summary>
public sealed class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    #region Checkout Session ID Validation

    [Fact]
    public void Validate_EmptyCheckoutSessionId_HasError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CheckoutSessionId");
    }

    [Fact]
    public void Validate_ValidCheckoutSessionId_NoError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "CheckoutSessionId");
    }

    #endregion

    #region Customer ID Validation

    [Fact]
    public void Validate_EmptyCustomerId_HasError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.Empty,
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_ValidCustomerId_NoError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "CustomerId");
    }

    #endregion

    #region Tenant ID Validation

    [Fact]
    public void Validate_EmptyTenantId_HasError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantId");
    }

    [Fact]
    public void Validate_ValidTenantId_NoError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "TenantId");
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public void Validate_AllFieldsEmpty_HasMultipleErrors()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.Empty,
            CustomerId = Guid.Empty,
            TenantId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(3);
    }

    [Fact]
    public void Validate_AllFieldsValid_IsValid()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CheckoutSessionId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
