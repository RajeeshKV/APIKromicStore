using KromicStore.Domain.Taxes.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Taxes.Entities;

public class TaxRuleTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _regionId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateRule()
    {
        // Act
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m, "Electronics tax");
        
        // Assert
        Assert.NotEqual(Guid.Empty, rule.Id);
        Assert.Equal(_tenantId, rule.TenantId);
        Assert.Equal(_regionId, rule.TaxRegionId);
        Assert.Equal("Electronics", rule.ProductCategory);
        Assert.Equal(0.15m, rule.TaxRate);
        Assert.Equal("Electronics tax", rule.Description);
        Assert.True(rule.IsActive);
        Assert.Null(rule.EffectiveFromUtc);
        Assert.Null(rule.EffectiveToUtc);
    }
    
    [Fact]
    public void Create_WithNullCategory_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            TaxRule.Create(_tenantId, _regionId, "", 0.15m));
    }
    
    [Fact]
    public void Create_WithInvalidTaxRate_ShouldThrow()
    {
        // Act & Assert - rate > 1
        Assert.Throws<ArgumentException>(() => 
            TaxRule.Create(_tenantId, _regionId, "Electronics", 1.5m));
        
        // Act & Assert - negative rate
        Assert.Throws<ArgumentException>(() => 
            TaxRule.Create(_tenantId, _regionId, "Electronics", -0.1m));
    }
    
    [Fact]
    public void Create_WithEffectiveDates_ShouldSet()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        
        // Act
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m, 
            effectiveFromUtc: from, effectiveToUtc: to);
        
        // Assert
        Assert.Equal(from, rule.EffectiveFromUtc);
        Assert.Equal(to, rule.EffectiveToUtc);
    }
    
    [Fact]
    public void Create_WithInvalidDateRange_ShouldThrow()
    {
        // Arrange
        var to = DateTime.UtcNow;
        var from = to.AddDays(1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m, 
                effectiveFromUtc: from, effectiveToUtc: to));
    }
    
    [Fact]
    public void UpdateRate_WithValidRate_ShouldUpdate()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        
        // Act
        rule.UpdateRate(0.20m);
        
        // Assert
        Assert.Equal(0.20m, rule.TaxRate);
    }
    
    [Fact]
    public void UpdateRate_WithInvalidRate_ShouldThrow()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => rule.UpdateRate(1.5m));
    }
    
    [Fact]
    public void SetEffectiveDateRange_WithValidRange_ShouldSet()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        var from = DateTime.UtcNow;
        var to = from.AddDays(60);
        
        // Act
        rule.SetEffectiveDateRange(from, to);
        
        // Assert
        Assert.Equal(from, rule.EffectiveFromUtc);
        Assert.Equal(to, rule.EffectiveToUtc);
    }
    
    [Fact]
    public void SetEffectiveDateRange_WithInvalidRange_ShouldThrow()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        var to = DateTime.UtcNow;
        var from = to.AddDays(1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => rule.SetEffectiveDateRange(from, to));
    }
    
    [Fact]
    public void SetEffectiveDateRange_WithNull_ShouldClearDates()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m,
            effectiveFromUtc: DateTime.UtcNow, effectiveToUtc: DateTime.UtcNow.AddDays(30));
        
        // Act
        rule.SetEffectiveDateRange(null, null);
        
        // Assert
        Assert.Null(rule.EffectiveFromUtc);
        Assert.Null(rule.EffectiveToUtc);
    }
    
    [Fact]
    public void IsEffectiveNow_WithinRange_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m,
            effectiveFromUtc: now.AddHours(-1), effectiveToUtc: now.AddHours(1));
        
        // Act
        var result = rule.IsEffectiveNow();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsEffectiveNow_BeforeRange_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m,
            effectiveFromUtc: now.AddDays(1), effectiveToUtc: now.AddDays(2));
        
        // Act
        var result = rule.IsEffectiveNow();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsEffectiveNow_AfterRange_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m,
            effectiveFromUtc: now.AddDays(-2), effectiveToUtc: now.AddDays(-1));
        
        // Act
        var result = rule.IsEffectiveNow();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsEffectiveNow_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m,
            effectiveFromUtc: now.AddHours(-1), effectiveToUtc: now.AddHours(1));
        rule.Deactivate();
        
        // Act
        var result = rule.IsEffectiveNow();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsEffectiveNow_NoDateRange_ShouldReturnTrueIfActive()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        
        // Act
        var result = rule.IsEffectiveNow();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        rule.Deactivate();
        
        // Act
        rule.Activate();
        
        // Assert
        Assert.True(rule.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var rule = TaxRule.Create(_tenantId, _regionId, "Electronics", 0.15m);
        
        // Act
        rule.Deactivate();
        
        // Assert
        Assert.False(rule.IsActive);
    }
}
