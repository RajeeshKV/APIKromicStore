using FluentAssertions;
using KromicStore.Domain.CustomerPortal.Entities;
using Xunit;

namespace KromicStore.Domain.Tests.CustomerPortal.Entities;

public class CustomerProfileTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithValidData_CreatesProfile()
    {
        // Arrange & Act
        var profile = CustomerProfile.Create(
            _tenantId,
            _customerId,
            "John",
            "Doe",
            "555-1234",
            new DateTime(1990, 1, 1));
        
        // Assert
        profile.Should().NotBeNull();
        profile.TenantId.Should().Be(_tenantId);
        profile.CustomerId.Should().Be(_customerId);
        profile.FirstName.Should().Be("John");
        profile.LastName.Should().Be("Doe");
        profile.PhoneNumber.Should().Be("555-1234");
        profile.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        profile.NewsletterOptIn.Should().BeFalse();
        profile.LoginCount.Should().Be(0);
        profile.LastLoginUtc.Should().BeNull();
    }
    
    [Fact]
    public void Create_WithNullFirstName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerProfile.Create(_tenantId, _customerId, null!, "Doe");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name is required*");
    }
    
    [Fact]
    public void Create_WithEmptyFirstName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerProfile.Create(_tenantId, _customerId, "   ", "Doe");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name is required*");
    }
    
    [Fact]
    public void Create_WithNullLastName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerProfile.Create(_tenantId, _customerId, "John", null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Last name is required*");
    }
    
    [Fact]
    public void Create_WithEmptyLastName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => CustomerProfile.Create(_tenantId, _customerId, "John", "   ");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Last name is required*");
    }
    
    [Fact]
    public void Create_WithFutureDateOfBirth_ThrowsArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddYears(1);
        
        // Act & Assert
        var act = () => CustomerProfile.Create(
            _tenantId,
            _customerId,
            "John",
            "Doe",
            dateOfBirth: futureDate);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Date of birth cannot be in the future*");
    }
    
    [Fact]
    public void Create_TrimWhitespace()
    {
        // Act
        var profile = CustomerProfile.Create(
            _tenantId,
            _customerId,
            "  John  ",
            "  Doe  ",
            "  555-1234  ");
        
        // Assert
        profile.FirstName.Should().Be("John");
        profile.LastName.Should().Be("Doe");
        profile.PhoneNumber.Should().Be("555-1234");
    }
    
    [Fact]
    public void GetFullName_ReturnsFormattedName()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act
        var fullName = profile.GetFullName();
        
        // Assert
        fullName.Should().Be("John Doe");
    }
    
    [Fact]
    public void UpdateProfile_WithValidData_Updates()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe", "555-1234");
        
        // Act
        profile.UpdateProfile("Jane", "Smith", "555-5678");
        
        // Assert
        profile.FirstName.Should().Be("Jane");
        profile.LastName.Should().Be("Smith");
        profile.PhoneNumber.Should().Be("555-5678");
    }
    
    [Fact]
    public void UpdateProfile_WithNullFirstName_ThrowsArgumentException()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act & Assert
        var act = () => profile.UpdateProfile(null!, "Smith");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name is required*");
    }
    
    [Fact]
    public void UpdateProfile_TrimWhitespace()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act
        profile.UpdateProfile("  Jane  ", "  Smith  ", "  555-5678  ");
        
        // Assert
        profile.FirstName.Should().Be("Jane");
        profile.LastName.Should().Be("Smith");
        profile.PhoneNumber.Should().Be("555-5678");
    }
    
    [Fact]
    public void SetNewsletterOptIn_True_Updates()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act
        profile.SetNewsletterOptIn(true);
        
        // Assert
        profile.NewsletterOptIn.Should().BeTrue();
    }
    
    [Fact]
    public void SetNewsletterOptIn_False_Updates()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        profile.SetNewsletterOptIn(true);
        
        // Act
        profile.SetNewsletterOptIn(false);
        
        // Assert
        profile.NewsletterOptIn.Should().BeFalse();
    }
    
    [Fact]
    public void UpdateNotificationPreferences_WithJson_Updates()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        var json = "{\"email\": true, \"sms\": false}";
        
        // Act
        profile.UpdateNotificationPreferences(json);
        
        // Assert
        profile.NotificationPreferences.Should().Be(json);
    }
    
    [Fact]
    public void UpdateNotificationPreferences_WithNull_Updates()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        profile.UpdateNotificationPreferences("{\"email\": true}");
        
        // Act
        profile.UpdateNotificationPreferences(null);
        
        // Assert
        profile.NotificationPreferences.Should().BeNull();
    }
    
    [Fact]
    public void RecordLogin_UpdatesLoginMetadata()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        var beforeLogin = DateTime.UtcNow;
        
        // Act
        profile.RecordLogin();
        var afterLogin = DateTime.UtcNow;
        
        // Assert
        profile.LoginCount.Should().Be(1);
        profile.LastLoginUtc.Should().BeOnOrAfter(beforeLogin);
        profile.LastLoginUtc.Should().BeOnOrBefore(afterLogin);
    }
    
    [Fact]
    public void RecordLogin_MultipleLogins_IncrementsCount()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act
        profile.RecordLogin();
        profile.RecordLogin();
        profile.RecordLogin();
        
        // Assert
        profile.LoginCount.Should().Be(3);
    }
    
    [Fact]
    public void GetAge_WithValidDateOfBirth_CalculatesCorrectly()
    {
        // Arrange
        var dob = new DateTime(1990, 1, 1);
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe", dateOfBirth: dob);
        
        // Act
        var age = profile.GetAge();
        
        // Assert
        age.Should().BeGreaterThan(30);
    }
    
    [Fact]
    public void GetAge_WithNullDateOfBirth_ReturnsNull()
    {
        // Arrange
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe");
        
        // Act
        var age = profile.GetAge();
        
        // Assert
        age.Should().BeNull();
    }
    
    [Fact]
    public void GetAge_BirthdayToday_CalculatesCorrectly()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var dob = new DateTime(today.Year - 30, today.Month, today.Day);
        var profile = CustomerProfile.Create(_tenantId, _customerId, "John", "Doe", dateOfBirth: dob);
        
        // Act
        var age = profile.GetAge();
        
        // Assert
        age.Should().Be(30);
    }
}
