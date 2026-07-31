using KromicStore.Infrastructure.Configuration;
using Xunit;

namespace KromicStore.Infrastructure.Tests.Configuration;

public sealed class MultiTenancyOptionsTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "store,admin,api",
            ExcludedSubdomains = "store,admin"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithDuplicateReservedSubdomains_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "store,admin,store",
            ExcludedSubdomains = "store"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("duplicate", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithDuplicateExcludedSubdomains_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "store,admin",
            ExcludedSubdomains = "store,admin,store"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("duplicate", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithInvalidSubdomainFormat_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "-invalid",
            ExcludedSubdomains = "store"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("Invalid", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsedReservedSubdomains_ParsesCorrectly()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "store, Admin, API"
        };

        // Act
        var parsed = options.ParsedReservedSubdomains;

        // Assert
        Assert.Equal(3, parsed.Count);
        Assert.Contains("store", parsed);
        Assert.Contains("admin", parsed); // Should be lowercased
        Assert.Contains("api", parsed);   // Should be lowercased
    }

    [Fact]
    public void IsReservedSubdomain_WithReservedValue_ReturnsTrue()
    {
        // Arrange
        var options = new MultiTenancyOptions { ReservedSubdomains = "store,admin" };

        // Act
        var result = options.IsReservedSubdomain("Store");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsReservedSubdomain_WithNonReservedValue_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions { ReservedSubdomains = "store,admin" };

        // Act
        var result = options.IsReservedSubdomain("custom");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExcludedSubdomain_WithExcludedValue_ReturnsTrue()
    {
        // Arrange
        var options = new MultiTenancyOptions { ExcludedSubdomains = "store,admin" };

        // Act
        var result = options.IsExcludedSubdomain("Admin");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExcludedSubdomain_WithNonExcludedValue_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions { ExcludedSubdomains = "store,admin" };

        // Act
        var result = options.IsExcludedSubdomain("tenant");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void IsReservedSubdomain_WithNullOrEmpty_ReturnsFalse(string subdomain)
    {
        // Arrange
        var options = new MultiTenancyOptions { ReservedSubdomains = "store" };

        // Act
        var result = options.IsReservedSubdomain(subdomain);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParsedReservedSubdomains_WithEmptyString_ReturnsEmptyCollection()
    {
        // Arrange
        var options = new MultiTenancyOptions { ReservedSubdomains = "" };

        // Act
        var parsed = options.ParsedReservedSubdomains;

        // Assert
        Assert.Empty(parsed);
    }

    [Fact]
    public void Validate_WithHyphenatedSubdomains_Succeeds()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "api-docs,api-gateway",
            ExcludedSubdomains = "api-admin"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithTrailingHyphen_ReturnsFalse()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "store-",
            ExcludedSubdomains = "admin"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithNumericSubdomains_Succeeds()
    {
        // Arrange
        var options = new MultiTenancyOptions
        {
            ReservedSubdomains = "api1,api2",
            ExcludedSubdomains = "store"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
    }
}
