using FluentAssertions;
using KromicStore.Application.CustomerPortal.Commands.AddAddress;
using Xunit;

namespace KromicStore.Application.Tests.CustomerPortal.Commands;

public class AddAddressCommandValidatorTests
{
    private readonly AddAddressValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidData_SuccessfulValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Label = "Home",
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            IsShippingAddress = true
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_WithEmptyCustomerId_FailsValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.Empty,
            Label = "Home",
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "US"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithNullLabel_FailsValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Label = null!,
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "US"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithInvalidCountryCode_FailsValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Label = "Home",
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "USA" // Should be 2 chars
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithoutShippingOrBilling_FailsValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Label = "Home",
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            IsShippingAddress = false,
            IsBillingAddress = false
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithBothShippingAndBilling_SuccessfulValidation()
    {
        // Arrange
        var command = new AddAddressCommand
        {
            CustomerId = Guid.NewGuid(),
            Label = "Home",
            Street = "123 Main St",
            City = "New York",
            StateCode = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            IsShippingAddress = true,
            IsBillingAddress = true
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
}
