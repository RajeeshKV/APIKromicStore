using FluentAssertions;
using KromicStore.Domain.CustomerPortal.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.CustomerPortal.Entities;

public class CustomerNotificationPreferenceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesPreference()
    {
        // Act
        var preference = CustomerNotificationPreference.Create(
            _tenantId,
            _customerId,
            NotificationType.OrderUpdate);
        
        // Assert
        preference.Should().NotBeNull();
        preference.TenantId.Should().Be(_tenantId);
        preference.CustomerId.Should().Be(_customerId);
        preference.NotificationType.Should().Be(NotificationType.OrderUpdate);
        preference.EmailEnabled.Should().BeTrue();
        preference.SMSEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeTrue();
        preference.InAppEnabled.Should().BeTrue();
        preference.Frequency.Should().Be(NotificationFrequency.RealTime);
    }
    
    [Theory]
    [InlineData(NotificationType.OrderUpdate)]
    [InlineData(NotificationType.ShipmentTracking)]
    [InlineData(NotificationType.NewProducts)]
    [InlineData(NotificationType.Promotions)]
    [InlineData(NotificationType.AccountUpdates)]
    [InlineData(NotificationType.Reviews)]
    [InlineData(NotificationType.Surveys)]
    public void Create_WithAllNotificationTypes_CreatesPreference(NotificationType type)
    {
        // Act
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, type);
        
        // Assert
        preference.NotificationType.Should().Be(type);
    }
    
    [Fact]
    public void IsEnabled_WithAnyChannelEnabled_ReturnsTrue()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act & Assert
        preference.IsEnabled().Should().BeTrue();
    }
    
    [Fact]
    public void IsEnabled_WithAllChannelsDisabled_ReturnsFalse()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.DisableAll();
        
        // Assert
        preference.IsEnabled().Should().BeFalse();
    }
    
    [Theory]
    [InlineData(NotificationChannel.Email, true)]
    [InlineData(NotificationChannel.SMS, false)]
    [InlineData(NotificationChannel.Push, true)]
    [InlineData(NotificationChannel.InApp, true)]
    public void IsChannelEnabled_ReturnsCorrectState(NotificationChannel channel, bool expectedEnabled)
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        var isEnabled = preference.IsChannelEnabled(channel);
        
        // Assert
        isEnabled.Should().Be(expectedEnabled);
    }
    
    [Fact]
    public void UpdateChannels_WithValidChannels_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.UpdateChannels(false, true, false, true);
        
        // Assert
        preference.EmailEnabled.Should().BeFalse();
        preference.SMSEnabled.Should().BeTrue();
        preference.PushEnabled.Should().BeFalse();
        preference.InAppEnabled.Should().BeTrue();
    }
    
    [Fact]
    public void UpdateChannels_WithAllChannelsDisabled_ThrowsInvalidOperationException()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act & Assert
        var act = () => preference.UpdateChannels(false, false, false, false);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*At least one notification channel must be enabled*");
    }
    
    [Fact]
    public void EnableChannel_EnablesEmail()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        preference.UpdateChannels(false, true, true, true);
        
        // Act
        preference.EnableChannel(NotificationChannel.Email);
        
        // Assert
        preference.EmailEnabled.Should().BeTrue();
    }
    
    [Fact]
    public void EnableChannel_EnablesSMS()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.EnableChannel(NotificationChannel.SMS);
        
        // Assert
        preference.SMSEnabled.Should().BeTrue();
    }
    
    [Fact]
    public void EnableChannel_EnablesPush()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        preference.UpdateChannels(false, false, false, true);
        
        // Act
        preference.EnableChannel(NotificationChannel.Push);
        
        // Assert
        preference.PushEnabled.Should().BeTrue();
    }
    
    [Fact]
    public void DisableChannel_WithMultipleChannels_Disables()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.DisableChannel(NotificationChannel.Email);
        
        // Assert
        preference.EmailEnabled.Should().BeFalse();
        preference.SMSEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeTrue();
        preference.InAppEnabled.Should().BeTrue();
    }
    
    [Fact]
    public void DisableChannel_WithOnlyOneChannelEnabled_ThrowsInvalidOperationException()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        preference.UpdateChannels(true, false, false, false);
        
        // Act & Assert
        var act = () => preference.DisableChannel(NotificationChannel.Email);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot disable the last notification channel*");
    }
    
    [Fact]
    public void SetFrequency_WithRealTime_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.SetFrequency(NotificationFrequency.RealTime);
        
        // Assert
        preference.Frequency.Should().Be(NotificationFrequency.RealTime);
    }
    
    [Fact]
    public void SetFrequency_WithDaily_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.SetFrequency(NotificationFrequency.Daily);
        
        // Assert
        preference.Frequency.Should().Be(NotificationFrequency.Daily);
    }
    
    [Fact]
    public void SetFrequency_WithWeekly_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.SetFrequency(NotificationFrequency.Weekly);
        
        // Assert
        preference.Frequency.Should().Be(NotificationFrequency.Weekly);
    }
    
    [Fact]
    public void SetFrequency_WithMonthly_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.SetFrequency(NotificationFrequency.Monthly);
        
        // Assert
        preference.Frequency.Should().Be(NotificationFrequency.Monthly);
    }
    
    [Fact]
    public void SetFrequency_WithNever_Updates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.SetFrequency(NotificationFrequency.Never);
        
        // Assert
        preference.Frequency.Should().Be(NotificationFrequency.Never);
    }
    
    [Fact]
    public void DisableAll_DisablesAllChannels()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.DisableAll();
        
        // Assert
        preference.EmailEnabled.Should().BeFalse();
        preference.SMSEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeFalse();
        preference.InAppEnabled.Should().BeFalse();
    }
    
    [Fact]
    public void EnableDefaults_SetsDefaultChannels()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        preference.UpdateChannels(false, true, false, false);
        preference.SetFrequency(NotificationFrequency.Daily);
        
        // Act
        preference.EnableDefaults();
        
        // Assert
        preference.EmailEnabled.Should().BeTrue();
        preference.SMSEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeFalse();
        preference.InAppEnabled.Should().BeTrue();
        preference.Frequency.Should().Be(NotificationFrequency.RealTime);
    }
    
    [Fact]
    public void MultipleOperations_SequentialUpdates()
    {
        // Arrange
        var preference = CustomerNotificationPreference.Create(_tenantId, _customerId, NotificationType.OrderUpdate);
        
        // Act
        preference.EnableChannel(NotificationChannel.SMS);
        preference.SetFrequency(NotificationFrequency.Weekly);
        preference.DisableChannel(NotificationChannel.Push);
        
        // Assert
        preference.EmailEnabled.Should().BeTrue();
        preference.SMSEnabled.Should().BeTrue();
        preference.PushEnabled.Should().BeFalse();
        preference.InAppEnabled.Should().BeTrue();
        preference.Frequency.Should().Be(NotificationFrequency.Weekly);
    }
}
