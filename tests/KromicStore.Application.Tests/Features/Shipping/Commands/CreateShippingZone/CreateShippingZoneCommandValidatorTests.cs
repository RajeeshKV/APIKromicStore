using FluentValidation.TestHelper;
using KromicStore.Application.Features.Shipping.Commands.CreateShippingZone;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shipping.Commands.CreateShippingZone;

public class CreateShippingZoneCommandValidatorTests
{
    private readonly CreateShippingZoneCommandValidator _validator = new();
    
    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "North America",
            Description = "US, Canada, Mexico",
            Countries = ["US", "CA", "MX"]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "",
            Countries = ["US"]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = new string('A', 201),
            Countries = ["US"]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Zone",
            Description = new string('A', 1001),
            Countries = ["US"]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
    
    [Fact]
    public void Validate_WithEmptyCountriesList_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Zone",
            Countries = []
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Countries);
    }
    
    [Fact]
    public void Validate_WithInvalidCountryCode_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Zone",
            Countries = ["USA"] // Too long
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Countries);
    }
    
    [Fact]
    public void Validate_WithLowercaseCountryCode_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Zone",
            Countries = ["us"] // Lowercase not allowed by validator
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Countries);
    }
    
    [Fact]
    public void Validate_WithMultipleCountries_ShouldPass()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Europe",
            Countries = ["DE", "FR", "IT", "ES", "GB"]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyCountryInList_ShouldFail()
    {
        // Arrange
        var command = new CreateShippingZoneCommand
        {
            Name = "Zone",
            Countries = ["US", ""]
        };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Countries);
    }
}
