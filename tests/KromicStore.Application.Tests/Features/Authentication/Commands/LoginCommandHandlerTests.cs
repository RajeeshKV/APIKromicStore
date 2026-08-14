using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.Login;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

/// <summary>
/// Tests for LoginCommandHandler.
/// Verifies login logic including credential validation, account checks, refresh token creation, and error cases.
/// </summary>
public sealed class LoginCommandHandlerTests
{
    private readonly Guid _tenantId;
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();
        _sut = new LoginCommandHandler(_dbContext, _passwordHasher, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLoginUser_WhenCredentialsValid()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "alice@example.com",
            passwordHash: "hashed-password",
            firstName: "Alice",
            lastName: "Smith");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "alice@example.com",
            Password: "SecurePass1!",
            DeviceName: "iPhone",
            IpAddress: "192.168.1.1");

        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be("alice@example.com");
        result.User.IsEmailVerified.Should().BeTrue();
        result.ExpiresInSeconds.Should().Be(3600);
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthenticationException_WhenPasswordInvalid()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "bob@example.com",
            passwordHash: "hashed-password",
            firstName: "Bob",
            lastName: "Jones");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("bob@example.com", "WrongPassword!", null, null);

        _passwordHasher.Verify("hashed-password", "WrongPassword!").Returns(false);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_ShouldThrowAuthenticationException_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "AnyPass1!", null, null);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_ShouldAllowLoginWithUnverifiedEmail_ButFlagNotVerified()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "charlie@example.com",
            passwordHash: "hashed-password",
            firstName: "Charlie",
            lastName: "Brown");
        // Not verified - but should still allow login
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("charlie@example.com", "SecurePass1!", null, null);
        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert - Login should succeed but IsEmailVerified flag should be false
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.User.IsEmailVerified.Should().BeFalse();
        result.User.Email.Should().Be("charlie@example.com");
        // Frontend should use IsEmailVerified=false to show verification banner
    }

    [Fact]
    public async Task Handle_ShouldThrowAccountLockedException_WhenUserInactive()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "diana@example.com",
            passwordHash: "hashed-password",
            firstName: "Diana",
            lastName: "Prince");
        user.MarkEmailVerified();
        user.Deactivate();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand("diana@example.com", "SecurePass1!", null, null);
        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);

        // Act
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AccountLockedException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task Handle_ShouldCreateRefreshToken_WhenDeviceProvided()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "eve@example.com",
            passwordHash: "hashed-password",
            firstName: "Eve",
            lastName: "Adams");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "eve@example.com",
            Password: "SecurePass1!",
            DeviceName: "MacBook",
            IpAddress: "192.168.1.50");

        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var refreshTokens = _dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToList();
        refreshTokens.Should().HaveCount(1);
        refreshTokens[0].DeviceName.Should().Be("MacBook");
        refreshTokens[0].IPAddress.Should().Be("192.168.1.50");
        refreshTokens[0].TokenHash.Should().Be("hashed-refresh-token");
        refreshTokens[0].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldRecordLastLoginTime()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "frank@example.com",
            passwordHash: "hashed-password",
            firstName: "Frank",
            lastName: "Castle");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "frank@example.com",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        var beforeLogin = DateTime.UtcNow;

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedUser = _dbContext.Users.First(u => u.Id == user.Id);
        updatedUser.LastLoginOnUtc.Should().NotBeNull();
        updatedUser.LastLoginOnUtc.Should().BeOnOrAfter(beforeLogin);
        updatedUser.LastLoginOnUtc.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_ShouldIncludeRolesInResponse()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "grace@example.com",
            passwordHash: "hashed-password",
            firstName: "Grace",
            lastName: "Hopper");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var role = Role.Create("Admin");
        _dbContext.AddEntity(role);

        var userRole = UserRole.Create(user.Id, role.Id);
        _dbContext.AddEntity(userRole);

        await _dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "grace@example.com",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.User.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_ShouldNormalizeEmailBeforeQuery()
    {
        // Arrange
        var user = User.CreateTenantUser(
            tenantId: _tenantId,
            email: "henry@example.com",
            passwordHash: "hashed-password",
            firstName: "Henry",
            lastName: "Wilson");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        // Login with uppercase and spaces
        var command = new LoginCommand(
            Email: "  HENRY@EXAMPLE.COM  ",
            Password: "SecurePass1!",
            DeviceName: null,
            IpAddress: null);

        _passwordHasher.Verify("hashed-password", "SecurePass1!").Returns(true);
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("token");
        _tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenService.HashToken("raw-refresh-token").Returns("hashed-refresh-token");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be("henry@example.com");
    }
}
