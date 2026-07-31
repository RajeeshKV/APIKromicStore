using FluentAssertions;
using KromicStore.Application.CustomerPortal.Commands.UpdateNotificationPreferences;
using KromicStore.Domain.CustomerPortal.Entities;
using Xunit;

namespace KromicStore.Application.Tests.CustomerPortal.Commands;

public class UpdateNotificationPreferencesCommandValidatorTests
{
    private readonly UpdateNotificationPreferencesValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidData_SuccessfulValidation()
    {
        // Arrange
        var command = new UpdateNotificationPreferencesCommand
        {
            CustomerId = Guid.NewGuid(),
            NotificationType = NotificationType.OrderUpdate,
            EmailEnabled = true,
            SMSEnabled = false,
            PushEnabled = true,
            InAppEnabled = true,
            Frequency = NotificationFrequency.RealTime
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_WithEmptyCustomerId_FailsValidation()
    {
        // Arrange
        var command = new UpdateNotificationPreferencesCommand
        {
            CustomerId = Guid.Empty,
            NotificationType = NotificationType.OrderUpdate,
            EmailEnabled = true
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithAllChannelsDisabled_FailsValidation()
    {
        // Arrange
        var command = new UpdateNotificationPreferencesCommand
        {
            CustomerId = Guid.NewGuid(),
            NotificationType = NotificationType.OrderUpdate,
            EmailEnabled = false,
            SMSEnabled = false,
            PushEnabled = false,
            InAppEnabled = false
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(NotificationType.OrderUpdate)]
    [InlineData(NotificationType.ShipmentTracking)]
    [InlineData(NotificationType.NewProducts)]
    [InlineData(NotificationType.Promotions)]
    public void Validate_WithValidNotificationType_SuccessfulValidation(NotificationType type)
    {
        // Arrange
        var command = new UpdateNotificationPreferencesCommand
        {
            CustomerId = Guid.NewGuid(),
            NotificationType = type,
            EmailEnabled = true,
            Frequency = NotificationFrequency.RealTime
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
}
