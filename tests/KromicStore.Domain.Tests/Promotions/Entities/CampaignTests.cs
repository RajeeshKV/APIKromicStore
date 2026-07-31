using KromicStore.Domain.Promotions.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.Promotions.Entities;

public class CampaignTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidInput_ShouldCreateCampaign()
    {
        // Arrange
        var from = DateTime.UtcNow;
        var to = from.AddDays(30);
        
        // Act
        var campaign = Campaign.Create(_tenantId, "Summer Sale", from, to, "Big summer discounts", 5);
        
        // Assert
        Assert.NotEqual(Guid.Empty, campaign.Id);
        Assert.Equal(_tenantId, campaign.TenantId);
        Assert.Equal("Summer Sale", campaign.Name);
        Assert.Equal(from, campaign.ValidFromUtc);
        Assert.Equal(to, campaign.ValidToUtc);
        Assert.Equal("Big summer discounts", campaign.Description);
        Assert.Equal(5, campaign.DisplayOrder);
        Assert.True(campaign.IsActive);
        Assert.Empty(campaign.DiscountIds);
    }
    
    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Campaign.Create(_tenantId, "", DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
    }
    
    [Fact]
    public void Create_WithInvalidDateRange_ShouldThrow()
    {
        // Arrange
        var to = DateTime.UtcNow;
        var from = to.AddDays(1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Campaign.Create(_tenantId, "Campaign", from, to));
    }
    
    [Fact]
    public void AddDiscount_WithValidId_ShouldAdd()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var discountId = Guid.NewGuid();
        
        // Act
        campaign.AddDiscount(discountId);
        
        // Assert
        Assert.Contains(discountId, campaign.DiscountIds);
    }
    
    [Fact]
    public void AddDiscount_WithEmptyId_ShouldThrow()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => campaign.AddDiscount(Guid.Empty));
    }
    
    [Fact]
    public void AddDiscount_WithDuplicateId_ShouldNotAddDuplicate()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var discountId = Guid.NewGuid();
        
        // Act
        campaign.AddDiscount(discountId);
        campaign.AddDiscount(discountId);
        
        // Assert
        Assert.Single(campaign.DiscountIds);
    }
    
    [Fact]
    public void AddDiscount_WithMultipleDiscounts_ShouldAddAll()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var discount1 = Guid.NewGuid();
        var discount2 = Guid.NewGuid();
        
        // Act
        campaign.AddDiscount(discount1);
        campaign.AddDiscount(discount2);
        
        // Assert
        Assert.Equal(2, campaign.DiscountIds.Count);
    }
    
    [Fact]
    public void RemoveDiscount_WithExistingId_ShouldRemove()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var discountId = Guid.NewGuid();
        campaign.AddDiscount(discountId);
        
        // Act
        campaign.RemoveDiscount(discountId);
        
        // Assert
        Assert.DoesNotContain(discountId, campaign.DiscountIds);
    }
    
    [Fact]
    public void RemoveDiscount_WithNonExistentId_ShouldNotThrow()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Summer Sale", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act & Assert - should not throw
        campaign.RemoveDiscount(Guid.NewGuid());
    }
    
    [Fact]
    public void IsValid_WhenActiveAndInRange_ShouldReturnTrue()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var campaign = Campaign.Create(_tenantId, "Campaign", from, to);
        
        // Act
        var result = campaign.IsValid();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsValid_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);
        var campaign = Campaign.Create(_tenantId, "Campaign", from, to);
        campaign.Deactivate();
        
        // Act
        var result = campaign.IsValid();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-2);
        var to = DateTime.UtcNow.AddDays(-1);
        var campaign = Campaign.Create(_tenantId, "Campaign", from, to);
        
        // Act
        var result = campaign.IsValid();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsValid_WhenNotYetStarted_ShouldReturnFalse()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(1);
        var to = DateTime.UtcNow.AddDays(2);
        var campaign = Campaign.Create(_tenantId, "Campaign", from, to);
        
        // Act
        var result = campaign.IsValid();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Campaign", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        campaign.Deactivate();
        
        // Act
        campaign.Activate();
        
        // Assert
        Assert.True(campaign.IsActive);
    }
    
    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var campaign = Campaign.Create(_tenantId, "Campaign", DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        
        // Act
        campaign.Deactivate();
        
        // Assert
        Assert.False(campaign.IsActive);
    }
}
