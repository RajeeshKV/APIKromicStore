using KromicStore.Domain.Shipping.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Shipping.Entities;

public class ShippingZoneTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateZone()
    {
        // Arrange & Act
        var zone = ShippingZone.Create(_tenantId, "North America", "Ships to US, Canada, Mexico");
        
        // Assert
        Assert.NotEqual(Guid.Empty, zone.Id);
        Assert.Equal(_tenantId, zone.TenantId);
        Assert.Equal("North America", zone.Name);
        Assert.Equal("Ships to US, Canada, Mexico", zone.Description);
        Assert.True(zone.IsActive);
        Assert.Empty(zone.Countries);
    }
    
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShippingZone.Create(_tenantId, "", "Description"));
    }
    
    [Fact]
    public void AddCountry_WithValidCode_ShouldAddToList()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Europe");
        
        // Act
        zone.AddCountry("DE");
        zone.AddCountry("FR");
        
        // Assert
        Assert.Equal(2, zone.Countries.Count);
        Assert.Contains("DE", zone.Countries);
        Assert.Contains("FR", zone.Countries);
    }
    
    [Fact]
    public void AddCountry_WithUpperCase_ShouldStoreAsUpperCase()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        
        // Act
        zone.AddCountry("us");
        
        // Assert
        Assert.Contains("US", zone.Countries);
    }
    
    [Fact]
    public void AddCountry_WithInvalidCode_ShouldThrow()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => zone.AddCountry("USA")); // Too long
        Assert.Throws<ArgumentException>(() => zone.AddCountry("")); // Empty
    }
    
    [Fact]
    public void AddCountry_WithDuplicateCode_ShouldNotAddDuplicate()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        
        // Act
        zone.AddCountry("GB");
        zone.AddCountry("GB");
        
        // Assert
        Assert.Single(zone.Countries);
    }
    
    [Fact]
    public void RemoveCountry_WithExistingCode_ShouldRemove()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        zone.AddCountry("US");
        zone.AddCountry("CA");
        
        // Act
        zone.RemoveCountry("US");
        
        // Assert
        Assert.Single(zone.Countries);
        Assert.DoesNotContain("US", zone.Countries);
    }
    
    [Fact]
    public void RemoveCountry_WithNonExistentCode_ShouldNotThrow()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        
        // Act & Assert - should not throw
        zone.RemoveCountry("XX");
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        zone.Deactivate();
        Assert.False(zone.IsActive);
        
        // Act
        zone.Activate();
        
        // Assert
        Assert.True(zone.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        Assert.True(zone.IsActive);
        
        // Act
        zone.Deactivate();
        
        // Assert
        Assert.False(zone.IsActive);
    }
    
    [Fact]
    public void CoversCountry_WithExistingCountry_ShouldReturnTrue()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        zone.AddCountry("DE");
        
        // Act
        var result = zone.CoversCountry("de");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CoversCountry_WithNonExistentCountry_ShouldReturnFalse()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        zone.AddCountry("US");
        
        // Act
        var result = zone.CoversCountry("FR");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void AddMethod_WithValidMethod_ShouldAdd()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        var method = ShippingMethod.Create(_tenantId, zone.Id, "Standard", 3, 5, "Standard Shipping");
        
        // Act
        zone.AddMethod(method);
        
        // Assert
        Assert.Single(zone.Methods);
    }
    
    [Fact]
    public void AddMethod_WithNull_ShouldThrow()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => zone.AddMethod(null!));
    }
    
    [Fact]
    public void AddMethod_WithDuplicateId_ShouldNotAddDuplicate()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        var method = ShippingMethod.Create(_tenantId, zone.Id, "Standard", 3, 5);
        
        // Act
        zone.AddMethod(method);
        zone.AddMethod(method);
        
        // Assert
        Assert.Single(zone.Methods);
    }
    
    [Fact]
    public void RemoveMethod_WithExistingMethod_ShouldRemove()
    {
        // Arrange
        var zone = ShippingZone.Create(_tenantId, "Test Zone");
        var method = ShippingMethod.Create(_tenantId, zone.Id, "Standard", 3, 5);
        zone.AddMethod(method);
        
        // Act
        zone.RemoveMethod(method.Id);
        
        // Assert
        Assert.Empty(zone.Methods);
    }
}
