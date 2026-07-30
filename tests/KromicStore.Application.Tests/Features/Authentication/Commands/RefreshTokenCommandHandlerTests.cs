using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.RefreshToken;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<RefreshTokenCommandHandler>>();
        _sut = new RefreshTokenCommandHandler(_dbContext, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldRotateToken_WhenTokenValid()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var oldToken = DomainRefreshToken.Create(user.Id, "old-token-hash", DateTime.UtcNow.AddDays(15), "iPhone", "192.168.1.1");
        _dbContext.AddEntity(oldToken);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand("raw-old-token", null, null);
        _tokenService.HashToken("raw-old-token").Returns("old-token-hash");
        _tokenService.GenerateAccessToken(Arg.Any<User>(), Arg.Any<IEnumerable<string>>()).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns("new-raw-token");
        _tokenService.HashToken("new-raw-token").Returns("new-token-hash");
        _tokenService.RefreshTokenExpirationDays.Returns(30);
        _tokenService.AccessTokenExpirationSeconds.Returns(3600);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        var oldStoredToken = _dbContext.RefreshTokens.First(rt => rt.TokenHash == "old-token-hash");
        oldStoredToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenNotFound()
    {
        var command = new RefreshTokenCommand("nonexistent-token", null, null);
        _tokenService.HashToken("nonexistent-token").Returns("nonexistent-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenExpired()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var expiredToken = DomainRefreshToken.Create(user.Id, "expired-token-hash", DateTime.UtcNow.AddDays(-1), null, null);
        _dbContext.AddEntity(expiredToken);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand("raw-expired-token", null, null);
        _tokenService.HashToken("raw-expired-token").Returns("expired-token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenRevoked()
    {
        var user = User.CreateTenantUser(_tenantId, "charlie@example.com", "hashed", "Charlie", "Brown");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var revokedToken = DomainRefreshToken.Create(user.Id, "revoked-token-hash", DateTime.UtcNow.AddDays(15), null, null);
        revokedToken.Revoke(DateTime.UtcNow);
        _dbContext.AddEntity(revokedToken);
        await _dbContext.SaveChangesAsync();

        var command = new RefreshTokenCommand("raw-revoked-token", null, null);
        _tokenService.HashToken("raw-revoked-token").Returns("revoked-token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
