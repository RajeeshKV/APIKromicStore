using FluentAssertions;
using KromicStore.Application.Features.Orders.Commands.ConfirmOrder;
using Xunit;

namespace KromicStore.Application.Tests.Features.Orders.Commands.ConfirmOrder;

/// <summary>
/// Validator tests for ConfirmOrderCommand.
/// Verifies validation rules for order confirmation.
/// </summary>
public sealed class ConfirmOrderCommandValidatorTests
{
    private readonly ConfirmOrderCommandValidator _validator;

    public ConfirmOrderCommandValidatorTests()
    {
        _validator = new ConfirmOrderCommandValidator();
    }

    #region Order ID Validation

    [Fact]
    public void Validate_EmptyOrderId_HasError()
    {
        // Arrange
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.Empty,
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
    }

    [Fact]
    public void Validate_ValidOrderId_NoError()
    {
        // Arrange
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "OrderId");
    }

    #endregion

    #region Tenant ID Validation

    [Fact]
    public void Validate_EmptyTenantId_HasError()
    {
        // Arrange
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.NewGuid(),
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
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.NewGuid(),
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
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.Empty,
            TenantId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(2);
    }

    [Fact]
    public void Validate_AllFieldsValid_IsValid()
    {
        // Arrange
        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
