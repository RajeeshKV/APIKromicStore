using FluentAssertions;
using FluentValidation;
using KromicStore.Application.CustomerPortal.Commands.UpdateProfile;
using Xunit;

namespace KromicStore.Application.Tests.CustomerPortal.Commands;

public class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidData_SuccessfulValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "555-1234"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_WithNullCustomerId_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.Empty,
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateProfileCommand.CustomerId));
    }
    
    [Fact]
    public void Validate_WithNullFirstName_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = null!,
            LastName = "Doe"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateProfileCommand.FirstName));
    }
    
    [Fact]
    public void Validate_WithNullLastName_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "John",
            LastName = null!
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateProfileCommand.LastName));
    }
    
    [Fact]
    public void Validate_WithFirstNameTooLong_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = new string('A', 101),
            LastName = "Doe"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithFutureDateOfBirth_FailsValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.UtcNow.AddYears(1)
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void Validate_WithValidPhoneNumber_SuccessfulValidation()
    {
        // Arrange
        var command = new UpdateProfileCommand
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "555-1234"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
}
