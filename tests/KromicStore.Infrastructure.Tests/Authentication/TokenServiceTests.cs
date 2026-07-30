using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Configuration;
using KromicStore.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace KromicStore.Infrastructure.Tests.Authentication;

public sealed class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly JwtOptions _options;

    public TokenServiceTests()
    {
        _options = new JwtOptions
        {
            Secret = "super-secret-key-that-is-at-least-32-characters-long-for-hs256",
            Issuer = "KromicStore",
            Audience = "KromicStoreAPI",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        var optionsMonitor = Options.Create(_options);
        _sut = new TokenService(optionsMonitor);
    }

    [Fact]
    public void GenerateAccessToken_ShouldCreateValidJwt()
    {
        // Arrange
        var user = User.CreateSuperUser("alice@example.com", "hash", "Alice", "Smith");
        var roles = new[] { "User" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        jwtToken.Should().NotBeNull();
        jwtToken!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void HashToken_ShouldProduceConsistentHash()
    {
        // Arrange
        const string token = "some-verification-token";

        // Act
        var hash1 = _sut.HashToken(token);
        var hash2 = _sut.HashToken(token);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashToken_ShouldProduceDifferentHashes_ForDifferentTokens()
    {
        // Arrange
        const string token1 = "token-one";
        const string token2 = "token-two";

        // Act
        var hash1 = _sut.HashToken(token1);
        var hash2 = _sut.HashToken(token2);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
