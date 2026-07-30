using KromicStore.Infrastructure.Services;

namespace KromicStore.Infrastructure.Tests.Authentication;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldProduceDifferentHash_ForSamePassword()
    {
        // Arrange
        const string password = "SecurePassword123!";

        // Act
        var hash1 = _sut.Hash(password);
        var hash2 = _sut.Hash(password);

        // Assert
        hash1.Should().NotBeEmpty();
        hash2.Should().NotBeEmpty();
        hash1.Should().NotBe(hash2); // Different salts each time
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        const string password = "SecurePassword123!";
        var hash = _sut.Hash(password);

        // Act
        var result = _sut.Verify(hash, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        const string password = "SecurePassword123!";
        const string wrongPassword = "WrongPassword456!";
        var hash = _sut.Hash(password);

        // Act
        var result = _sut.Verify(hash, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenHashInvalid()
    {
        // Arrange
        const string password = "AnyPassword123!";
        const string invalidHash = "not-a-valid-bcrypt-hash";

        // Act
        var result = _sut.Verify(invalidHash, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_ShouldThrow_WhenPasswordNull()
    {
        // Arrange
        string? password = null;

        // Act
        Func<string> act = () => _sut.Hash(password!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_ShouldThrow_WhenPasswordNull()
    {
        // Arrange
        const string hash = "some-hash";

        // Act
        Func<bool> act = () => _sut.Verify(hash, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_ShouldThrow_WhenHashNull()
    {
        // Arrange
        const string password = "AnyPassword123!";

        // Act
        Func<bool> act = () => _sut.Verify(null!, password);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
