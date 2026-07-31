using KromicStore.Domain.Promotions.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Promotions.Entities;

public class CouponTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _discountId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateCoupon()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        
        // Act
        var coupon = Coupon.Create(_tenantId, "SAVE20", _discountId, from, to, "Save 20%");
        
        // Assert
        Assert.NotEqual(Guid.Empty, coupon.Id);
        Assert.Equal(_tenantId, coupon.TenantId);
        Assert.Equal("SAVE20", coupon.Code);
        Assert.Equal(_discountId, coupon.DiscountId);
        Assert.Equal(from, coupon.ValidFromUtc);
        Assert.Equal(to, coupon.ValidToUtc);
        Assert.Equal("Save 20%", coupon.Description);
        Assert.True(coupon.IsActive);
        Assert.Equal(0, coupon.CurrentUsageCount);
    }
    
    [Fact]
    public void Create_WithNullCode_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Coupon.Create(_tenantId, "", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
    }
    
    [Fact]
    public void Create_WithInvalidDateRange_ShouldThrow()
    {
        // Arrange
        var to = DateTime.UtcNow;
        var from = to.AddDays(1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Coupon.Create(_tenantId, "CODE", _discountId, from, to));
    }
    
    [Fact]
    public void Create_WithCodeUpperCase()
    {
        // Act
        var coupon = Coupon.Create(_tenantId, "save20", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Assert
        Assert.Equal("SAVE20", coupon.Code);
    }
    
    [Fact]
    public void Create_WithUsageLimits_ShouldSet()
    {
        // Act
        var coupon = Coupon.Create(_tenantId, "LIMITED", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            maxUsageCount: 100, maxUsagePerCustomer: 2);
        
        // Assert
        Assert.Equal(100, coupon.MaxUsageCount);
        Assert.Equal(2, coupon.MaxUsagePerCustomer);
    }
    
    [Fact]
    public void Create_WithInvalidUsageLimit_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Coupon.Create(_tenantId, "CODE", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
                maxUsageCount: 0));
    }
    
    [Fact]
    public void Create_WithMinimumOrderValue_ShouldSet()
    {
        // Act
        var coupon = Coupon.Create(_tenantId, "MINORDER", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            minimumOrderValue: 50m);
        
        // Assert
        Assert.Equal(50m, coupon.MinimumOrderValue);
    }
    
    [Fact]
    public void CanBeUsed_WhenActiveAndValid_ShouldReturnTrue()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "VALID", _discountId, from, to);
        
        // Act
        var result = coupon.CanBeUsed();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void CanBeUsed_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "INACTIVE", _discountId, from, to);
        coupon.Deactivate();
        
        // Act
        var result = coupon.CanBeUsed();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CanBeUsed_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-2);
        var to = DateTime.UtcNow.AddDays(-1);
        var coupon = Coupon.Create(_tenantId, "EXPIRED", _discountId, from, to);
        
        // Act
        var result = coupon.CanBeUsed();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CanBeUsed_WhenUsageLimitExceeded_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "LIMITED", _discountId, from, to, maxUsageCount: 1);
        coupon.IncrementUsage();
        
        // Act
        var result = coupon.CanBeUsed();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void CanBeUsed_WithCustomerUsageLimitExceeded_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "PERUSER", _discountId, from, to, maxUsagePerCustomer: 1);
        
        // Act
        var result = coupon.CanBeUsed(currentCustomerUsageCount: 1);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IncrementUsage_WhenValid_ShouldIncrement()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "VALID", _discountId, from, to);
        
        // Act
        coupon.IncrementUsage();
        
        // Assert
        Assert.Equal(1, coupon.CurrentUsageCount);
    }
    
    [Fact]
    public void IncrementUsage_WhenInvalid_ShouldThrow()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var coupon = Coupon.Create(_tenantId, "EXPIRED", _discountId, from, to, maxUsageCount: 1);
        coupon.IncrementUsage();
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => coupon.IncrementUsage());
    }
    
    [Fact]
    public void AppliesToCategory_WithAllCategories_ShouldReturnTrue()
    {
        // Arrange
        var coupon = Coupon.Create(_tenantId, "ALL", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        var result = coupon.AppliesToCategory("Electronics");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void AppliesToCategory_WithSpecificCategories_ShouldMatch()
    {
        // Arrange
        var coupon = Coupon.Create(_tenantId, "ELECTRONICS", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            applicableCategories: "Electronics,Computers");
        
        // Act
        var result = coupon.AppliesToCategory("Electronics");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void AppliesToCategory_WithNonMatchingCategory_ShouldReturnFalse()
    {
        // Arrange
        var coupon = Coupon.Create(_tenantId, "ELECTRONICS", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1),
            applicableCategories: "Electronics");
        
        // Act
        var result = coupon.AppliesToCategory("Clothing");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var coupon = Coupon.Create(_tenantId, "CODE", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        coupon.Deactivate();
        
        // Act
        coupon.Activate();
        
        // Assert
        Assert.True(coupon.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var coupon = Coupon.Create(_tenantId, "CODE", _discountId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        coupon.Deactivate();
        
        // Assert
        Assert.False(coupon.IsActive);
    }
}
