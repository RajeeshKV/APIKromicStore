using KromicStore.Domain.Taxes.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Taxes.Entities;

public class TaxRegionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateRegion()
    {
        // Act
        var region = TaxRegion.Create(_tenantId, "United States", "US", isTaxInclusive: false);
        
        // Assert
        Assert.NotEqual(Guid.Empty, region.Id);
        Assert.Equal(_tenantId, region.TenantId);
        Assert.Equal("United States", region.Name);
        Assert.Equal("US", region.CountryCode);
        Assert.False(region.IsTaxInclusive);
        Assert.True(region.IsActive);
        Assert.Null(region.StateCode);
    }
    
    [Fact]
    public void Create_WithStateCode_ShouldSetState()
    {
        // Act
        var region = TaxRegion.Create(_tenantId, "California", "US", isTaxInclusive: false, stateCode: "CA");
        
        // Assert
        Assert.Equal("CA", region.StateCode);
    }
    
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            TaxRegion.Create(_tenantId, "", "US", false));
    }
    
    [Fact]
    public void Create_WithInvalidCountryCode_ShouldThrow()
    {
        // Act & Assert - code too long
        Assert.Throws<ArgumentException>(() => 
            TaxRegion.Create(_tenantId, "Test", "USA", false));
        
        // Act & Assert - empty code
        Assert.Throws<ArgumentException>(() => 
            TaxRegion.Create(_tenantId, "Test", "", false));
    }
    
    [Fact]
    public void Create_WithLowercaseCountryCode_ShouldStoreAsUppercase()
    {
        // Act
        var region = TaxRegion.Create(_tenantId, "Test", "de", false);
        
        // Assert
        Assert.Equal("DE", region.CountryCode);
    }
    
    [Fact]
    public void AddRule_WithValidRule_ShouldAdd()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        
        // Act
        region.AddRule(rule);
        
        // Assert
        Assert.Single(region.Rules);
    }
    
    [Fact]
    public void AddRule_WithNull_ShouldThrow()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => region.AddRule(null!));
    }
    
    [Fact]
    public void AddRule_WithMultipleRules_ShouldAddAll()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule1 = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        var rule2 = TaxRule.Create(_tenantId, region.Id, "Clothing", 0.0m);
        
        // Act
        region.AddRule(rule1);
        region.AddRule(rule2);
        
        // Assert
        Assert.Equal(2, region.Rules.Count);
    }
    
    [Fact]
    public void RemoveRule_WithExistingRule_ShouldRemove()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        region.AddRule(rule);
        
        // Act
        region.RemoveRule(rule.Id);
        
        // Assert
        Assert.Empty(region.Rules);
    }
    
    [Fact]
    public void GetTaxRate_WithExistingCategory_ShouldReturnRate()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        region.AddRule(rule);
        
        // Act
        var rate = region.GetTaxRate("Electronics");
        
        // Assert
        Assert.Equal(0.08m, rate);
    }
    
    [Fact]
    public void GetTaxRate_WithNonExistentCategory_ShouldReturnZero()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        region.AddRule(rule);
        
        // Act
        var rate = region.GetTaxRate("Clothing");
        
        // Assert
        Assert.Equal(0, rate);
    }
    
    [Fact]
    public void GetTaxRate_WithInactiveRule_ShouldReturnZero()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        var rule = TaxRule.Create(_tenantId, region.Id, "Electronics", 0.08m);
        rule.Deactivate();
        region.AddRule(rule);
        
        // Act
        var rate = region.GetTaxRate("Electronics");
        
        // Assert
        Assert.Equal(0, rate);
    }
    
    [Fact]
    public void GetTaxRate_WithNullCategory_ShouldReturnZero()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        
        // Act
        var rate = region.GetTaxRate(null);
        
        // Assert
        Assert.Equal(0, rate);
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        region.Deactivate();
        
        // Act
        region.Activate();
        
        // Assert
        Assert.True(region.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var region = TaxRegion.Create(_tenantId, "US", "US", false);
        
        // Act
        region.Deactivate();
        
        // Assert
        Assert.False(region.IsActive);
    }
    
    [Fact]
    public void TaxInclusiveFlag_ShouldStoreCorrectly()
    {
        // Act
        var inclusiveRegion = TaxRegion.Create(_tenantId, "EU", "DE", isTaxInclusive: true);
        var exclusiveRegion = TaxRegion.Create(_tenantId, "US", "US", isTaxInclusive: false);
        
        // Assert
        Assert.True(inclusiveRegion.IsTaxInclusive);
        Assert.False(exclusiveRegion.IsTaxInclusive);
    }
}
