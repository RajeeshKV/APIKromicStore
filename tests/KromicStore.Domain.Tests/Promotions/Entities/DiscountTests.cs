using KromicStore.Domain.Promotions.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Promotions.Entities;

public class DiscountTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    
    [Fact]
    public void CreateFixedAmountDiscount_WithValidInput_ShouldCreate()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        
        // Act
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Save $10", 10m, from, to, "Save 10 dollars");
        
        // Assert
        Assert.NotEqual(Guid.Empty, discount.Id);
        Assert.Equal(_tenantId, discount.TenantId);
        Assert.Equal("Save $10", discount.Name);
        Assert.Equal(DiscountType.FixedAmount, discount.Type);
        Assert.Equal(10m, discount.FixedAmount);
        Assert.Equal("Save 10 dollars", discount.Description);
        Assert.True(discount.IsActive);
    }
    
    [Fact]
    public void CreateFixedAmountDiscount_WithInvalidAmount_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 0, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
    }
    
    [Fact]
    public void CreatePercentageDiscount_WithValidInput_ShouldCreate()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        
        // Act
        var discount = Discount.CreatePercentageDiscount(_tenantId, "Save 20%", 0.20m, from, to);
        
        // Assert
        Assert.Equal(DiscountType.PercentageAmount, discount.Type);
        Assert.Equal(0.20m, discount.PercentageAmount);
    }
    
    [Fact]
    public void CreatePercentageDiscount_WithInvalidPercentage_ShouldThrow()
    {
        // Act & Assert - > 1
        Assert.Throws<ArgumentException>(() => 
            Discount.CreatePercentageDiscount(_tenantId, "Discount", 1.5m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
        
        // Act & Assert - negative
        Assert.Throws<ArgumentException>(() => 
            Discount.CreatePercentageDiscount(_tenantId, "Discount", -0.1m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
    }
    
    [Fact]
    public void CreatePercentageDiscount_WithMaxAmount_ShouldSet()
    {
        // Act
        var discount = Discount.CreatePercentageDiscount(_tenantId, "Discount", 0.50m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            maxDiscountAmount: 100m);
        
        // Assert
        Assert.Equal(100m, discount.MaxDiscountAmount);
    }
    
    [Fact]
    public void CreateFreeShippingDiscount_WithValidInput_ShouldCreate()
    {
        // Act
        var discount = Discount.CreateFreeShippingDiscount(_tenantId, "Free Shipping", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Assert
        Assert.Equal(DiscountType.FreeShipping, discount.Type);
    }
    
    [Fact]
    public void CreateFreeShippingDiscount_WithMinimum_ShouldSet()
    {
        // Act
        var discount = Discount.CreateFreeShippingDiscount(_tenantId, "Free Shipping Over 50", DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            minimumOrderValue: 50m);
        
        // Assert
        Assert.Equal(50m, discount.FreeShippingMinimum);
    }
    
    [Fact]
    public void IsValid_WhenActiveAndInRange_ShouldReturnTrue()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, from, to);
        
        // Act
        var result = discount.IsValid();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValid_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, from, to);
        discount.Deactivate();
        
        // Act
        var result = discount.IsValid();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-2);
        var to = DateTime.UtcNow.AddDays(-1);
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, from, to);
        
        // Act
        var result = discount.IsValid();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CalculateDiscountAmount_ForFixedAmount_ShouldReturnCost()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var amount = discount.CalculateDiscountAmount(100m);
        
        // Assert
        Assert.Equal(10m, amount);
    }
    
    [Fact]
    public void CalculateDiscountAmount_ForFixedAmountCappedByOrderAmount_ShouldReturnMin()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 50m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var amount = discount.CalculateDiscountAmount(30m);
        
        // Assert
        Assert.Equal(30m, amount); // Capped at order amount
    }
    
    [Fact]
    public void CalculateDiscountAmount_ForPercentage_ShouldReturnCalculatedAmount()
    {
        // Arrange
        var discount = Discount.CreatePercentageDiscount(_tenantId, "Discount", 0.20m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var amount = discount.CalculateDiscountAmount(100m);
        
        // Assert
        Assert.Equal(20m, amount);
    }
    
    [Fact]
    public void CalculateDiscountAmount_ForPercentageWithMax_ShouldNotExceedMax()
    {
        // Arrange
        var discount = Discount.CreatePercentageDiscount(_tenantId, "Discount", 0.50m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            maxDiscountAmount: 25m);
        
        // Act
        var amount = discount.CalculateDiscountAmount(100m);
        
        // Assert
        Assert.Equal(25m, amount);
    }
    
    [Fact]
    public void CalculateDiscountAmount_ForFreeShipping_ShouldReturnZero()
    {
        // Arrange
        var discount = Discount.CreateFreeShippingDiscount(_tenantId, "Free Shipping", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var amount = discount.CalculateDiscountAmount(100m);
        
        // Assert
        Assert.Equal(0m, amount);
    }
    
    [Fact]
    public void AppliesToProduct_WithAllProducts_ShouldReturnTrue()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var result = discount.AppliesToProduct("ProductA", "CategoryX");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void AppliesToProduct_WithSpecificProducts_ShouldMatch()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            applicableProductIds: "ProductA,ProductB");
        
        // Act
        var result = discount.AppliesToProduct("ProductA");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void AppliesToProduct_WithSpecificCategories_ShouldMatch()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            applicableCategories: "Electronics,Computers");
        
        // Act
        var result = discount.AppliesToProduct("SomeProduct", "Electronics");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IncrementUsage_WhenValid_ShouldIncrement()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, from, to);
        
        // Act
        discount.IncrementUsage();
        
        // Assert
        Assert.Equal(1, discount.CurrentUsageCount);
    }
    
    [Fact]
    public void Activate_ShouldActivate()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        discount.Deactivate();
        
        // Act
        discount.Activate();
        
        // Assert
        Assert.True(discount.IsActive);
    }
    
    [Fact]
    public void Deactivate_ShouldDeactivate()
    {
        // Arrange
        var discount = Discount.CreateFixedAmountDiscount(_tenantId, "Discount", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        discount.Deactivate();
        
        // Assert
        Assert.False(discount.IsActive);
    }
}
