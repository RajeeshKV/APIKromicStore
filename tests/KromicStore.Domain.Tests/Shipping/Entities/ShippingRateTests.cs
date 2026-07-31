using KromicStore.Domain.Shipping.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Shipping.Entities;

public class ShippingRateTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _methodId = Guid.NewGuid();
    
    [Fact]
    public void CreateWeightBased_WithValidInput_ShouldCreate()
    {
        // Act
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, 10m);
        
        // Assert
        Assert.NotEqual(Guid.Empty, rate.Id);
        Assert.Equal(_tenantId, rate.TenantId);
        Assert.Equal(_methodId, rate.ShippingMethodId);
        Assert.Equal(0, rate.MinWeight);
        Assert.Equal(5, rate.MaxWeight);
        Assert.Equal(10m, rate.Cost);
        Assert.True(rate.IsWeightBased);
        Assert.True(rate.IsActive);
    }
    
    [Fact]
    public void CreateWeightBased_WithNegativeWeight_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateWeightBased(_tenantId, _methodId, -1, 5, 10m));
    }
    
    [Fact]
    public void CreateWeightBased_WithMaxLessThanMin_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateWeightBased(_tenantId, _methodId, 5, 2, 10m));
    }
    
    [Fact]
    public void CreateWeightBased_WithNegativeCost_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, -10m));
    }
    
    [Fact]
    public void CreateValueBased_WithValidInput_ShouldCreate()
    {
        // Act
        var rate = ShippingRate.CreateValueBased(_tenantId, _methodId, 0, 100, 5m);
        
        // Assert
        Assert.NotEqual(Guid.Empty, rate.Id);
        Assert.Equal(_tenantId, rate.TenantId);
        Assert.Equal(_methodId, rate.ShippingMethodId);
        Assert.Equal(0, rate.MinOrderValue);
        Assert.Equal(100, rate.MaxOrderValue);
        Assert.Equal(5m, rate.Cost);
        Assert.False(rate.IsWeightBased);
        Assert.True(rate.IsActive);
    }
    
    [Fact]
    public void CreateValueBased_WithNegativeValue_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateValueBased(_tenantId, _methodId, -10, 100, 5m));
    }
    
    [Fact]
    public void CreateValueBased_WithMaxLessThanMin_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateValueBased(_tenantId, _methodId, 100, 50, 5m));
    }
    
    [Fact]
    public void CreateValueBased_WithNegativeCost_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ShippingRate.CreateValueBased(_tenantId, _methodId, 0, 100, -5m));
    }
    
    [Fact]
    public void UpdateCost_WithValidCost_ShouldUpdate()
    {
        // Arrange
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, 10m);
        
        // Act
        rate.UpdateCost(15m);
        
        // Assert
        Assert.Equal(15m, rate.Cost);
    }
    
    [Fact]
    public void UpdateCost_WithNegativeCost_ShouldThrow()
    {
        // Arrange
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, 10m);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => rate.UpdateCost(-5m));
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, 10m);
        rate.Deactivate();
        
        // Act
        rate.Activate();
        
        // Assert
        Assert.True(rate.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 0, 5, 10m);
        
        // Act
        rate.Deactivate();
        
        // Assert
        Assert.False(rate.IsActive);
    }
    
    [Fact]
    public void WeightBasedRate_StoredCorrectly_ValueRangeEmpty()
    {
        // Act
        var rate = ShippingRate.CreateWeightBased(_tenantId, _methodId, 2, 8, 20m);
        
        // Assert
        Assert.Equal(0, rate.MinOrderValue);
        Assert.Equal(0, rate.MaxOrderValue);
    }
    
    [Fact]
    public void ValueBasedRate_StoredCorrectly_WeightRangeEmpty()
    {
        // Act
        var rate = ShippingRate.CreateValueBased(_tenantId, _methodId, 50, 200, 10m);
        
        // Assert
        Assert.Equal(0, rate.MinWeight);
        Assert.Equal(0, rate.MaxWeight);
    }
}
