using KromicStore.Domain.Identity;

namespace KromicStore.Domain.Tests.Identity;

public sealed class EmailVerificationTokenTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddHours(24);

        var token = EmailVerificationToken.Create(userId, "hash-xyz", expiry);

        token.UserId.Should().Be(userId);
        token.TokenHash.Should().Be("hash-xyz");
        token.IsConsumed.Should().BeFalse();
        token.ConsumedOnUtc.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdEmpty()
    {
        var act = () => EmailVerificationToken.Create(Guid.Empty, "hash", DateTime.UtcNow.AddHours(24));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTokenHashEmpty()
    {
        var act = () => EmailVerificationToken.Create(Guid.NewGuid(), "", DateTime.UtcNow.AddHours(24));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Consume_ShouldSetConsumedOnUtc()
    {
        var token    = Create();
        var consumed = DateTime.UtcNow;

        token.Consume(consumed);

        token.IsConsumed.Should().BeTrue();
        token.ConsumedOnUtc.Should().BeCloseTo(consumed, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Consume_ShouldBeIdempotent_DoesNotOverwriteFirstConsumption()
    {
        var token  = Create();
        var first  = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var second = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        token.Consume(first);
        token.Consume(second); // must not overwrite

        token.ConsumedOnUtc.Should().Be(first);
    }

    [Fact]
    public void IsConsumed_ShouldBeFalse_BeforeConsumption()
    {
        var token = Create();
        token.IsConsumed.Should().BeFalse();
    }

    private static EmailVerificationToken Create() =>
        EmailVerificationToken.Create(Guid.NewGuid(), "hash", DateTime.UtcNow.AddHours(24));
}
