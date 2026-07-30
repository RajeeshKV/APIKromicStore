using KromicStore.Domain.Identity;

namespace KromicStore.Domain.Tests.Identity;

public sealed class UserTests
{
    // ── Factory methods ───────────────────────────────────────────────────────

    [Fact]
    public void CreateTenantUser_ShouldSetProperties()
    {
        var tenantId = Guid.NewGuid();
        var user = User.CreateTenantUser(tenantId, "Alice@Example.COM", "hash", "Alice", "Smith");

        user.TenantId.Should().Be(tenantId);
        user.Email.Should().Be("alice@example.com");   // normalised
        user.FirstName.Should().Be("Alice");
        user.LastName.Should().Be("Smith");
        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
        user.TokenVersion.Should().Be(1);
    }

    [Fact]
    public void CreateSuperUser_ShouldHaveNullTenantId()
    {
        var user = User.CreateSuperUser("admin@kromic.in", "hash", "Super", "Admin");

        user.TenantId.Should().BeNull();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateTenantUser_ShouldThrow_WhenEmailEmpty()
    {
        var act = () => User.CreateTenantUser(Guid.NewGuid(), "", "hash", "Alice", "Smith");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateTenantUser_ShouldThrow_WhenTenantIdEmpty()
    {
        var act = () => User.CreateTenantUser(Guid.Empty, "a@b.com", "hash", "Alice", "Smith");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateTenantUser_ShouldThrow_WhenFirstNameEmpty()
    {
        var act = () => User.CreateTenantUser(Guid.NewGuid(), "a@b.com", "hash", "", "Smith");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateTenantUser_ShouldThrow_WhenPasswordHashEmpty()
    {
        var act = () => User.CreateTenantUser(Guid.NewGuid(), "a@b.com", "", "Alice", "Smith");
        act.Should().Throw<ArgumentException>();
    }

    // ── Email verification ────────────────────────────────────────────────────

    [Fact]
    public void MarkEmailVerified_ShouldSetIsEmailVerifiedTrue()
    {
        var user = CreateUser();

        user.MarkEmailVerified();

        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public void MarkEmailVerified_ShouldBeIdempotent()
    {
        var user = CreateUser();
        user.MarkEmailVerified();
        user.MarkEmailVerified(); // second call should not throw

        user.IsEmailVerified.Should().BeTrue();
    }

    // ── Login recording ───────────────────────────────────────────────────────

    [Fact]
    public void RecordLogin_ShouldSetLastLoginOnUtc()
    {
        var user = CreateUser();
        var loginTime = DateTime.UtcNow;

        user.RecordLogin(loginTime);

        user.LastLoginOnUtc.Should().BeCloseTo(loginTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordLogin_ShouldEnsureUtcKind()
    {
        var user = CreateUser();
        var localTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);

        user.RecordLogin(localTime);

        user.LastLoginOnUtc!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = CreateUser();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldIncrementTokenVersion()
    {
        var user = CreateUser();
        var versionBefore = user.TokenVersion;

        user.Deactivate();

        user.TokenVersion.Should().Be(versionBefore + 1);
    }

    // ── Password change ───────────────────────────────────────────────────────

    [Fact]
    public void ChangePasswordHash_ShouldUpdatePasswordHash()
    {
        var user = CreateUser();

        user.ChangePasswordHash("new-hash");

        user.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public void ChangePasswordHash_ShouldIncrementTokenVersion()
    {
        var user = CreateUser();
        var versionBefore = user.TokenVersion;

        user.ChangePasswordHash("new-hash");

        user.TokenVersion.Should().Be(versionBefore + 1);
    }

    [Fact]
    public void ChangePasswordHash_ShouldThrow_WhenHashEmpty()
    {
        var user = CreateUser();

        var act = () => user.ChangePasswordHash("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangePasswordHash_ShouldThrow_WhenHashWhitespace()
    {
        var user = CreateUser();

        var act = () => user.ChangePasswordHash("   ");
        act.Should().Throw<ArgumentException>();
    }

    // ── Email normalisation ───────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldTrimAndLowercaseEmail()
    {
        var user = User.CreateTenantUser(Guid.NewGuid(), "  ALICE@EXAMPLE.COM  ", "hash", "Alice", "Smith");

        user.Email.Should().Be("alice@example.com");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User CreateUser() =>
        User.CreateTenantUser(Guid.NewGuid(), "alice@example.com", "hash", "Alice", "Smith");
}
