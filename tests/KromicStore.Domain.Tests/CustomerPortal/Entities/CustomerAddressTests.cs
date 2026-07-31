using FluentAssertions;
using KromicStore.Domain.CustomerPortal.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.CustomerPortal.Entities;

public class CustomerAddressTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidShippingAddress_CreatesAddress()
    {
        // Act
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            "555-1234",
            isShipping: true,
            isBilling: false);
        
        // Assert
        address.Should().NotBeNull();
        address.TenantId.Should().Be(_tenantId);
        address.CustomerId.Should().Be(_customerId);
        address.Label.Should().Be("Home");
        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.StateCode.Should().Be("NY");
        address.PostalCode.Should().Be("10001");
        address.CountryCode.Should().Be("US");
        address.PhoneNumber.Should().Be("555-1234");
        address.IsShippingAddress.Should().BeTrue();
        address.IsBillingAddress.Should().BeFalse();
        address.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void Create_WithValidBillingAddress_CreatesAddress()
    {
        // Act
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Office",
            "456 Business Ave",
            "Boston",
            "MA",
            "02101",
            "US",
            isShipping: false,
            isBilling: true);
        
        // Assert
        address.IsShippingAddress.Should().BeFalse();
        address.IsBillingAddress.Should().BeTrue();
    }
    
    [Fact]
    public void Create_WithBothShippingAndBilling_CreatesAddress()
    {
        // Act
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            isShipping: true,
            isBilling: true);
        
        // Assert
        address.IsShippingAddress.Should().BeTrue();
        address.IsBillingAddress.Should().BeTrue();
    }
    
    [Fact]
    public void Create_WithoutShippingOrBilling_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            isShipping: false,
            isBilling: false);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Address must be marked as shipping or billing*");
    }
    
    [Fact]
    public void Create_WithNullLabel_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            null!,
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Label is required*");
    }
    
    [Fact]
    public void Create_WithNullStreet_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            null!,
            "New York",
            "NY",
            "10001",
            "US");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Street is required*");
    }
    
    [Fact]
    public void Create_WithNullCity_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            null!,
            "NY",
            "10001",
            "US");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*City is required*");
    }
    
    [Fact]
    public void Create_WithNullStateCode_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            null!,
            "10001",
            "US");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*State code is required*");
    }
    
    [Fact]
    public void Create_WithNullPostalCode_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            null!,
            "US");
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Postal code is required*");
    }
    
    [Fact]
    public void Create_WithInvalidCountryCode_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "USA"); // Should be 2 chars
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Country code must be 2 characters*");
    }
    
    [Fact]
    public void Create_NormalizesCountryCodeToUpperCase()
    {
        // Act
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "ny",
            "10001",
            "us");
        
        // Assert
        address.CountryCode.Should().Be("US");
        address.StateCode.Should().Be("NY");
    }
    
    [Fact]
    public void GetFormattedAddress_ReturnsCorrectFormat()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US");
        
        // Act
        var formatted = address.GetFormattedAddress();
        
        // Assert
        formatted.Should().Be("123 Main St, New York, NY 10001, US");
    }
    
    [Fact]
    public void UpdateAddress_WithValidData_Updates()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            "555-1234");
        
        // Act
        address.UpdateAddress("456 Oak Ave", "Boston", "MA", "02101", "555-5678");
        
        // Assert
        address.Street.Should().Be("456 Oak Ave");
        address.City.Should().Be("Boston");
        address.StateCode.Should().Be("MA");
        address.PostalCode.Should().Be("02101");
        address.PhoneNumber.Should().Be("555-5678");
    }
    
    [Fact]
    public void SetAsDefaultShipping_OnShippingAddress_Sets()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            isShipping: true,
            isBilling: false);
        
        // Act
        address.SetAsDefaultShipping();
        
        // Assert
        address.IsDefaultShipping.Should().BeTrue();
    }
    
    [Fact]
    public void SetAsDefaultShipping_OnNonShippingAddress_ThrowsInvalidOperationException()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Office",
            "456 Business Ave",
            "Boston",
            "MA",
            "02101",
            "US",
            isShipping: false,
            isBilling: true);
        
        // Act & Assert
        var act = () => address.SetAsDefaultShipping();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Address must be marked as shipping address*");
    }
    
    [Fact]
    public void SetAsDefaultBilling_OnBillingAddress_Sets()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Office",
            "456 Business Ave",
            "Boston",
            "MA",
            "02101",
            "US",
            isShipping: false,
            isBilling: true);
        
        // Act
        address.SetAsDefaultBilling();
        
        // Assert
        address.IsDefaultBilling.Should().BeTrue();
    }
    
    [Fact]
    public void UnsetAsDefaultShipping_Unsets()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            isShipping: true);
        address.SetAsDefaultShipping();
        
        // Act
        address.UnsetAsDefaultShipping();
        
        // Assert
        address.IsDefaultShipping.Should().BeFalse();
    }
    
    [Fact]
    public void UnsetAsDefaultBilling_Unsets()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US",
            isBilling: true);
        address.SetAsDefaultBilling();
        
        // Act
        address.UnsetAsDefaultBilling();
        
        // Assert
        address.IsDefaultBilling.Should().BeFalse();
    }
    
    [Fact]
    public void Activate_Activates()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US");
        address.Deactivate();
        
        // Act
        address.Activate();
        
        // Assert
        address.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void Deactivate_Deactivates()
    {
        // Arrange
        var address = CustomerAddress.Create(
            _tenantId,
            _customerId,
            "Home",
            "123 Main St",
            "New York",
            "NY",
            "10001",
            "US");
        
        // Act
        address.Deactivate();
        
        // Assert
        address.IsActive.Should().BeFalse();
    }
}
