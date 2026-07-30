using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.ResetPassword;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly ResetPasswordCommandHandler _sut;

    public ResetPasswordCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<ResetPasswordCommandHandler>>();
        _sut = new ResetPasswordCommandHandler(_dbContext, _passwordHasher, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldResetPassword_WhenTokenValid()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "old-hash", "Alice", "Smith");
        var token = PasswordResetToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(3));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand("raw-token", "NewPass1!", "NewPass1!");
        _tokenService.HashToken("raw-token").Returns("token-hash");
        _passwordHasher.Hash("NewPass1!").Returns("new-hash");

        await _sut.Handle(command, CancellationToken.None);

        var updatedUser = _dbContext.Users.First(u => u.Id == user.Id);
        updatedUser.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenNotFound()
    {
        var command = new ResetPasswordCommand("raw-nonexistent-token", "NewPass1!", "NewPass1!");
        _tokenService.HashToken("raw-nonexistent-token").Returns("nonexistent-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenExpired()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "old-hash", "Bob", "Jones");
        var expiredToken = PasswordResetToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(-1));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(expiredToken);
        await _dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand("raw-token", "NewPass1!", "NewPass1!");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenAlreadyConsumed()
    {
        var user = User.CreateTenantUser(_tenantId, "charlie@example.com", "old-hash", "Charlie", "Brown");
        var token = PasswordResetToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(3));
        token.Consume(DateTime.UtcNow);

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand("raw-token", "NewPass1!", "NewPass1!");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldRevokeAllRefreshTokens_ToForceRelogin()
    {
        var user = User.CreateTenantUser(_tenantId, "diana@example.com", "old-hash", "Diana", "Prince");
        var refreshToken1 = DomainRefreshToken.Create(user.Id, "token-1", DateTime.UtcNow.AddDays(15), "iPhone", "192.168.1.1");
        var refreshToken2 = DomainRefreshToken.Create(user.Id, "token-2", DateTime.UtcNow.AddDays(15), "iPad", "192.168.1.2");
        var resetToken = PasswordResetToken.Create(user.Id, "reset-token-hash", DateTime.UtcNow.AddDays(3));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(refreshToken1);
        _dbContext.AddEntity(refreshToken2);
        _dbContext.AddEntity(resetToken);
        await _dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand("raw-reset-token", "NewPass1!", "NewPass1!");
        _tokenService.HashToken("raw-reset-token").Returns("reset-token-hash");
        _passwordHasher.Hash("NewPass1!").Returns("new-hash");

        await _sut.Handle(command, CancellationToken.None);

        var allRefreshTokens = _dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToList();
        allRefreshTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }
}
