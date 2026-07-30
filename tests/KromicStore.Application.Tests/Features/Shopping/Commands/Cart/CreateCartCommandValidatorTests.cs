using FluentAssertions;
using KromicStore.Application.Features.Shopping.Commands.CreateCart;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Validator tests for CreateCartCommand.
/// Verifies validation rules for cart creation.
/// </summary>
public sealed class CreateCartCommandValidatorTests
{
    private readonly CreateCartCommandValidator _validator;

    public CreateCartCommandValidatorTests()
    {
        _validator = new CreateCartCommandValidator();
    }

    #region Currency Validation Tests

    [Fact]
    public void Validate_EmptyCurrency_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_NullCurrency_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: null!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_InvalidCurrencyLength_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: "US"); // Only 2 chars instead of 3

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidCurrencyCode_NoError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_DifferentValidCurrencies_NoError()
    {
        // Arrange
        var validCurrencies = new[] { "USD", "EUR", "GBP", "JPY", "CAD", "AUD" };

        // Act & Assert
        foreach (var currency in validCurrencies)
        {
            var command = new CreateCartCommand(
                CustomerId: Guid.NewGuid(),
                Currency: currency);
            var result = _validator.Validate(command);
            result.Errors.Should().NotContain(e => e.PropertyName == "Currency");
        }
    }

    #endregion

    #region Customer ID or Session ID Validation Tests

    [Fact]
    public void Validate_WithCustomerId_NoError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSessionId_NoError()
    {
        // Arrange
        var command = new CreateCartCommand(
            AnonymousSessionId: "guest-session-123",
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithBothCustomerIdAndSessionId_NoError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            AnonymousSessionId: "guest-session",
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNeitherCustomerIdNorSessionId_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: null,
            AnonymousSessionId: null,
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.Empty,
            AnonymousSessionId: null,
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptySessionId_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: null,
            AnonymousSessionId: "",
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithWhitespaceSessionId_HasError()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: null,
            AnonymousSessionId: "   ",
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Session ID Length Validation Tests

    [Fact]
    public void Validate_SessionIdExceedsMaxLength_HasError()
    {
        // Arrange
        var longSessionId = new string('A', 256);
        var command = new CreateCartCommand(
            AnonymousSessionId: longSessionId,
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_SessionIdAtMaxLength_NoError()
    {
        // Arrange
        var sessionId = new string('A', 255);
        var command = new CreateCartCommand(
            AnonymousSessionId: sessionId,
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Validate_ValidCommandForCustomer_NoErrors()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            Currency: "USD");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandForGuest_NoErrors()
    {
        // Arrange
        var command = new CreateCartCommand(
            AnonymousSessionId: "guest-session-xyz",
            Currency: "EUR");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithAllFields_NoErrors()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: Guid.NewGuid(),
            AnonymousSessionId: "guest-session",
            Currency: "GBP");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
