using KromicStore.Domain.Shipping.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Shipping.Entities;

public class ShippingMethodTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _zoneId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateMethod()
    {
        // Act
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Express", 1, 2, "Express Delivery", 10);
        
        // Assert
        Assert.NotEqual(Guid.Empty, method.Id);
        Assert.Equal(_tenantId, method.TenantId);
        Assert.Equal(_zoneId, method.ShippingZoneId);
        Assert.Equal("Express", method.Name);
        Assert.Equal(1, method.EstimatedDaysMin);
        Assert.Equal(2, method.EstimatedDaysMax);
        Assert.Equal("Express Delivery", method.Description);
        Assert.Equal(10, method.DisplayOrder);
        Assert.True(method.IsActive);
    }
    
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingMethod.Create(_tenantId, _zoneId, "", 1, 2));
    }
    
    [Fact]
    public void Create_WithNegativeDays_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingMethod.Create(_tenantId, _zoneId, "Standard", -1, 5));
    }
    
    [Fact]
    public void Create_WithMaxLessThanMin_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingMethod.Create(_tenantId, _zoneId, "Standard", 5, 2));
    }
    
    [Fact]
    public void AddRate_WithValidRate_ShouldAdd()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate = ShippingRate.CreateWeightBased(_tenantId, method.Id, 0, 1, 10m);
        
        // Act
        method.AddRate(rate);
        
        // Assert
        Assert.Single(method.Rates);
    }
    
    [Fact]
    public void AddRate_WithNull_ShouldThrow()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => method.AddRate(null!));
    }
    
    [Fact]
    public void AddRate_WithOverlappingWeightRanges_ShouldThrow()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate1 = ShippingRate.CreateWeightBased(_tenantId, method.Id, 0, 5, 10m);
        var rate2 = ShippingRate.CreateWeightBased(_tenantId, method.Id, 3, 8, 15m);
        
        // Act & Assert
        method.AddRate(rate1);
        Assert.Throws<InvalidOperationException>(() => method.AddRate(rate2));
    }
    
    [Fact]
    public void CalculateShippingCost_WithWeightRateMatch_ShouldReturnCost()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate = ShippingRate.CreateWeightBased(_tenantId, method.Id, 0, 10, 25m);
        method.AddRate(rate);
        
        // Act
        var cost = method.CalculateShippingCost(5, 0);
        
        // Assert
        Assert.Equal(25m, cost);
    }
    
    [Fact]
    public void CalculateShippingCost_WithValueRateMatch_ShouldReturnCost()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate = ShippingRate.CreateValueBased(_tenantId, method.Id, 0, 100, 5m);
        method.AddRate(rate);
        
        // Act
        var cost = method.CalculateShippingCost(0, 50);
        
        // Assert
        Assert.Equal(5m, cost);
    }
    
    [Fact]
    public void CalculateShippingCost_WithNoMatch_ShouldReturnNull()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate = ShippingRate.CreateWeightBased(_tenantId, method.Id, 0, 5, 10m);
        method.AddRate(rate);
        
        // Act
        var cost = method.CalculateShippingCost(10, 0); // Weight 10 is outside range
        
        // Assert
        Assert.Null(cost);
    }
    
    [Fact]
    public void CalculateShippingCost_WithNegativeValues_ShouldReturnNull()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        
        // Act
        var cost = method.CalculateShippingCost(-1, 0);
        
        // Assert
        Assert.Null(cost);
    }
    
    [Fact]
    public void RemoveRate_WithExistingRate_ShouldRemove()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        var rate = ShippingRate.CreateWeightBased(_tenantId, method.Id, 0, 5, 10m);
        method.AddRate(rate);
        
        // Act
        method.RemoveRate(rate.Id);
        
        // Assert
        Assert.Empty(method.Rates);
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        method.Deactivate();
        
        // Act
        method.Activate();
        
        // Assert
        Assert.True(method.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var method = ShippingMethod.Create(_tenantId, _zoneId, "Standard", 3, 5);
        
        // Act
        method.Deactivate();
        
        // Assert
        Assert.False(method.IsActive);
    }
}
