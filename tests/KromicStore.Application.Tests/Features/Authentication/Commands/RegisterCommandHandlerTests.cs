using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.Register;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

/// <summary>
/// Tests for RegisterCommandHandler.
/// Verifies user registration including password hashing, email verification token creation,
/// refresh token creation, role assignment, and conflict detection.
/// </summary>
public sealed class RegisterCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<RegisterCommandHandler>>();
        _sut = new RegisterCommandHandler(
            _dbContext,
            _passwordHasher,
            _tokenService,
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenValidRequest()
    {
        // Arrange
        var command = new RegisterCommand(
            FirstName:  "Alice",
            LastName:   "Smith",
            Email:      "alice@example.com",
            Password:   "SecurePass1!",
            DeviceName: "iPhone",
            IpAddress:  "192.168.1.1");

        const string hashedPassword = "hashed-password-value";
        _passwordHasher.Hash("SecurePass1!").Returns(hashedPassword);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be("alice@example.com");
        result.User.FirstName.Should().Be("Alice");
        result.User.LastName.Should().Be("Smith");
        result.User.IsEmailVerified.Should().BeFalse();

        var user = _dbContext.Users.First(u => u.Id == result.User.Id);
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be(hashedPassword);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = User.CreateSuperUser(
            email: "alice@example.com",
            passwordHash: "any-hash",
            firstName: "Bob",
            lastName: "Jones");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var command = new RegisterCommand(
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice@example.com",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_ShouldCreateRefreshToken_WhenDeviceProvided()
    {
        // Arrange
        var command = new RegisterCommand(
            FirstName: "Diana",
            LastName: "Prince",
            Email: "diana@example.com",
            Password: "SecurePass1!",
            DeviceName: "iPad",
            IpAddress: "10.0.0.1");

        _passwordHasher.Hash("SecurePass1!").Returns("hashed-password");
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var refreshTokens = _dbContext.RefreshTokens.Where(rt => rt.UserId == result.User.Id).ToList();
        refreshTokens.Should().HaveCount(1);
        refreshTokens[0].DeviceName.Should().Be("iPad");
        refreshTokens[0].IPAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task Handle_ShouldCreateEmailVerificationToken()
    {
        // Arrange
        var command = new RegisterCommand(
            FirstName: "Eve",
            LastName: "Adams",
            Email: "eve@example.com",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        _passwordHasher.Hash("SecurePass1!").Returns("hashed");
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var verificationTokens = _dbContext.EmailVerificationTokens.Where(vt => vt.UserId == result.User.Id).ToList();
        verificationTokens.Should().HaveCount(1);
        verificationTokens[0].IsConsumed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldAssignTenantAdminRole()
    {
        // Arrange
        var command = new RegisterCommand(
            FirstName: "Frank",
            LastName: "Castle",
            Email: "frank@example.com",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        var role = Role.Create(Roles.TenantAdmin);
        _dbContext.AddEntity(role);
        await _dbContext.SaveChangesAsync();

        _passwordHasher.Hash("SecurePass1!").Returns("hashed");
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.User.Roles.Should().Contain(Roles.TenantAdmin);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeEmailBeforeStorage()
    {
        // Arrange
        var command = new RegisterCommand(
            FirstName: "Grace",
            LastName: "Hopper",
            Email: "  GRACE@EXAMPLE.COM  ",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        _passwordHasher.Hash("SecurePass1!").Returns("hashed");
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.User.Email.Should().Be("grace@example.com");
        var user = _dbContext.Users.First(u => u.Id == result.User.Id);
        user.Email.Should().Be("grace@example.com");
    }
}
