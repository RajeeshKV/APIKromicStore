using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.Logout;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class LogoutCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<LogoutCommandHandler>>();
        _sut = new LogoutCommandHandler(_dbContext, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        var token = DomainRefreshToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(15), "iPhone", "192.168.1.1");

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new LogoutCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var storedToken = _dbContext.RefreshTokens.First(rt => rt.TokenHash == "token-hash");
        storedToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenTokenAlreadyRevoked()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        var token = DomainRefreshToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(15), null, null);
        token.Revoke(DateTime.UtcNow);

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new LogoutCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenTokenNotFound()
    {
        var command = new LogoutCommand("raw-nonexistent-token");
        _tokenService.HashToken("raw-nonexistent-token").Returns("nonexistent-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
