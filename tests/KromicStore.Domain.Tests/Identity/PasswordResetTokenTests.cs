using KromicStore.Domain.Identity;

namespace KromicStore.Domain.Tests.Identity;

public sealed class PasswordResetTokenTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(45);

        var token = PasswordResetToken.Create(userId, "reset-hash", expiry);

        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be("reset-hash");
        token.IsConsumed.Should().BeFalse();
        token.ConsumedOnUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdEmpty()
    {
        var act = () => PasswordResetToken.Create(Guid.Empty, "hash", DateTime.UtcNow.AddMinutes(45));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTokenHashEmpty()
    {
        var act = () => PasswordResetToken.Create(Guid.NewGuid(), "  ", DateTime.UtcNow.AddMinutes(45));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Consume_ShouldMarkConsumed()
    {
        var token = Create();
        var now   = DateTime.UtcNow;

        token.Consume(now);

        token.IsConsumed.Should().BeTrue();
        token.ConsumedOnUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Consume_IsIdempotent_DoesNotOverwriteFirstConsumption()
    {
        var token  = Create();
        var first  = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var second = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        token.Consume(first);
        token.Consume(second);

        token.ConsumedOnUtc.Should().Be(first);
    }

    private static PasswordResetToken Create() =>
        PasswordResetToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddMinutes(45));
}
