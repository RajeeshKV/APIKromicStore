using KromicStore.Infrastructure.Configuration;
using Xunit;

namespace KromicStore.Infrastructure.Tests.Configuration;

public sealed class CorsOptionsTests
{
    [Fact]
    public void Validate_WithValidOrigins_ReturnsSuccess()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "https://store.kromic.in,https://admin.kromic.in,http://localhost:3000"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithEmptyOrigins_ReturnsTrue()
    {
        // Arrange - Empty origins are allowed (can be configured via environment variables)
        var options = new CorsOptions { AllowedOrigins = "" };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithInvalidUrl_ReturnsFalse()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "not-a-url" };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("Invalid", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithDuplicateOrigins_ReturnsFalse()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "https://store.kromic.in,https://admin.kromic.in,https://store.kromic.in"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("duplicate", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParsedAllowedOrigins_ParsesCorrectly()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "https://store.kromic.in, http://localhost:3000 , https://admin.kromic.in"
        };

        // Act
        var parsed = options.ParsedAllowedOrigins;

        // Assert
        Assert.Equal(3, parsed.Count);
        Assert.Contains("https://store.kromic.in", parsed);
        Assert.Contains("http://localhost:3000", parsed);
        Assert.Contains("https://admin.kromic.in", parsed);
    }

    [Fact]
    public void IsOriginAllowed_WithAllowedOrigin_ReturnsTrue()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://store.kromic.in,http://localhost:3000" };

        // Act
        var result = options.IsOriginAllowed("https://store.kromic.in");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOriginAllowed_WithNonAllowedOrigin_ReturnsFalse()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://store.kromic.in" };

        // Act
        var result = options.IsOriginAllowed("https://other.kromic.in");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOriginAllowed_WithCaseDifference_ReturnsTrue()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://store.kromic.in" };

        // Act
        var result = options.IsOriginAllowed("HTTPS://STORE.KROMIC.IN");

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void IsOriginAllowed_WithNullOrEmpty_ReturnsFalse(string origin)
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://store.kromic.in" };

        // Act
        var result = options.IsOriginAllowed(origin);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Validate_WithHttpAndHttpsOrigins_Succeeds()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "http://localhost:3000,https://store.kromic.in,http://localhost:5173"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_WithOriginIncludingPort_Succeeds()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "https://store.kromic.in:8443,http://localhost:5173"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Validate_WithMissingScheme_ReturnsFalse()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "store.kromic.in" };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ParsedAllowedOrigins_TrimsWhitespace()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "  https://store.kromic.in  ,  http://localhost:3000  "
        };

        // Act
        var parsed = options.ParsedAllowedOrigins;

        // Assert
        Assert.Equal(2, parsed.Count);
        Assert.DoesNotContain("  ", parsed.First());
    }

    [Fact]
    public void Validate_WithWildcardSubdomain_Succeeds()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://*.kromic.in" };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithWildcardPort_Succeeds()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "http://localhost:*" };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithMixedWildcardAndExplicit_Succeeds()
    {
        // Arrange
        var options = new CorsOptions
        {
            AllowedOrigins = "https://*.kromic.in,http://localhost:3000,https://admin.example.com"
        };

        // Act
        var (isValid, error) = options.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void IsOriginAllowed_WithWildcardSubdomain_MatchesSubdomains()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://*.kromic.in" };

        // Act
        var storeResult = options.IsOriginAllowed("https://store.kromic.in");
        var adminResult = options.IsOriginAllowed("https://admin.kromic.in");
        var otherResult = options.IsOriginAllowed("https://kromic.in"); // No subdomain

        // Assert
        Assert.True(storeResult);
        Assert.True(adminResult);
        Assert.False(otherResult); // Doesn't match - no subdomain
    }

    [Fact]
    public void IsOriginAllowed_WithWildcardPort_MatchesPorts()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "http://localhost:*" };

        // Act
        var port3000 = options.IsOriginAllowed("http://localhost:3000");
        var port5173 = options.IsOriginAllowed("http://localhost:5173");
        var https = options.IsOriginAllowed("https://localhost:3000"); // Different scheme

        // Assert
        Assert.True(port3000);
        Assert.True(port5173);
        Assert.False(https); // Doesn't match - different scheme
    }

    [Fact]
    public void IsOriginAllowed_WithWildcardPattern_IsCaseInsensitive()
    {
        // Arrange
        var options = new CorsOptions { AllowedOrigins = "https://*.kromic.in" };

        // Act
        var result1 = options.IsOriginAllowed("https://STORE.KROMIC.IN");
        var result2 = options.IsOriginAllowed("HTTPS://ADMIN.KROMIC.IN");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }
}
