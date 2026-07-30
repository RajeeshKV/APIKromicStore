using KromicStore.Domain.Identity;

namespace KromicStore.Domain.Tests.Identity;

public sealed class RefreshTokenTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var userId  = Guid.NewGuid();
        var expiry  = DateTime.UtcNow.AddDays(7);
        var token   = RefreshToken.Create(userId, "hash-abc", expiry, "Chrome/Win", "1.2.3.4");

        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be("hash-abc");
        token.DeviceName.Should().Be("Chrome/Win");
        token.IPAddress.Should().Be("1.2.3.4");
        token.IsRevoked.Should().BeFalse();
        token.RevokedOnUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdEmpty()
    {
        var act = () => RefreshToken.Create(Guid.Empty, "hash", DateTime.UtcNow.AddDays(7), null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTokenHashEmpty()
    {
        var act = () => RefreshToken.Create(Guid.NewGuid(), "", DateTime.UtcNow.AddDays(7), null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldEnsureExpiryIsUtc()
    {
        var unspecified = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", unspecified, null, null);

        token.ExpiresOnUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    // ── Revocation ────────────────────────────────────────────────────────────

    [Fact]
    public void Revoke_ShouldSetRevokedOnUtc()
    {
        var token = CreateToken();
        var revokeTime = DateTime.UtcNow;

        token.Revoke(revokeTime);

        token.IsRevoked.Should().BeTrue();
        token.RevokedOnUtc.Should().BeCloseTo(revokeTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_ShouldBeIdempotent()
    {
        var token = CreateToken();
        var firstRevoke = DateTime.UtcNow;
        token.Revoke(firstRevoke);

        var secondRevoke = DateTime.UtcNow.AddMinutes(5);
        token.Revoke(secondRevoke); // should not overwrite

        token.RevokedOnUtc.Should().BeCloseTo(firstRevoke, TimeSpan.FromSeconds(1));
    }

    // ── Expiry ────────────────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenNotYetExpired()
    {
        var token = CreateToken(expiresInDays: 7);

        token.IsExpired(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenPastExpiry()
    {
        var token = CreateToken(expiresInDays: -1);

        token.IsExpired(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExactlyAtExpiry()
    {
        var expiry = DateTime.UtcNow.AddSeconds(-1);
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", expiry, null, null);

        token.IsExpired(DateTime.UtcNow).Should().BeTrue();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static RefreshToken CreateToken(int expiresInDays = 7) =>
        RefreshToken.Create(Guid.NewGuid(), "token-hash", DateTime.UtcNow.AddDays(expiresInDays), null, null);
}
