using FluentAssertions;
using KromicStore.Application.Features.Orders.Commands.InitializePayment;
using Xunit;

namespace KromicStore.Application.Tests.Features.Orders.Commands.InitializePayment;

/// <summary>
/// Validator tests for InitializePaymentCommand.
/// Verifies validation rules for payment initialization.
/// </summary>
public sealed class InitializePaymentCommandValidatorTests
{
    private readonly InitializePaymentCommandValidator _validator;

    public InitializePaymentCommandValidatorTests()
    {
        _validator = new InitializePaymentCommandValidator();
    }

    #region Order ID Validation

    [Fact]
    public void Validate_EmptyOrderId_HasError()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
    }

    #endregion

    #region Customer ID Validation

    [Fact]
    public void Validate_EmptyCustomerId_HasError()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.Empty,
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    #endregion

    #region Tenant ID Validation

    [Fact]
    public void Validate_EmptyTenantId_HasError()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.Empty,
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantId");
    }

    #endregion

    #region Amount Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void Validate_ZeroOrNegativeAmount_HasError(decimal amount)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = amount,
            Currency = "USD",
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(9999999.99)]
    public void Validate_PositiveAmount_NoError(decimal amount)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = amount,
            Currency = "USD",
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Amount");
    }

    #endregion

    #region Currency Validation

    [Theory]
    [InlineData("")]
    [InlineData("U")]
    [InlineData("USDA")]
    [InlineData("US")]
    public void Validate_InvalidCurrency_HasError(string currency)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = currency,
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public void Validate_ValidCurrency_NoError(string currency)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = currency,
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Currency");
    }

    #endregion

    #region Payment Method Validation

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPaymentMethod_HasError(string method)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = method
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PaymentMethod");
    }

    [Theory]
    [InlineData("CreditCard")]
    [InlineData("DebitCard")]
    [InlineData("PayPal")]
    [InlineData("BankTransfer")]
    public void Validate_ValidPaymentMethod_NoError(string method)
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = method
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "PaymentMethod");
    }

    [Fact]
    public void Validate_PaymentMethodExceeds100Chars_HasError()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = new string('A', 101)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PaymentMethod");
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public void Validate_AllFieldsInvalid_HasMultipleErrors()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.Empty,
            CustomerId = Guid.Empty,
            TenantId = Guid.Empty,
            Amount = -100m,
            Currency = "INVALID",
            PaymentMethod = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void Validate_AllFieldsValid_IsValid()
    {
        // Arrange
        var command = new InitializePaymentCommand
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethod = "CreditCard",
            Provider = "Stripe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
