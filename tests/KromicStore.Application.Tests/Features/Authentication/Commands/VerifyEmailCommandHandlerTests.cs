using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.VerifyEmail;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class VerifyEmailCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ITokenService _tokenService;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;
    private readonly VerifyEmailCommandHandler _sut;

    public VerifyEmailCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<VerifyEmailCommandHandler>>();
        _sut = new VerifyEmailCommandHandler(_dbContext, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldVerifyEmail_WhenTokenValid()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        var token = EmailVerificationToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(3));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyEmailCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var updatedUser = _dbContext.Users.First(u => u.Id == user.Id);
        updatedUser.IsEmailVerified.Should().BeTrue();
        var consumedToken = _dbContext.EmailVerificationTokens.First(t => t.TokenHash == "token-hash");
        consumedToken.IsConsumed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenNotFound()
    {
        var command = new VerifyEmailCommand("raw-nonexistent-token");
        _tokenService.HashToken("raw-nonexistent-token").Returns("nonexistent-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenExpired()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        var expiredToken = EmailVerificationToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(-1));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(expiredToken);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyEmailCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenAlreadyConsumed()
    {
        var user = User.CreateTenantUser(_tenantId, "charlie@example.com", "hashed", "Charlie", "Brown");
        var token = EmailVerificationToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(3));
        token.Consume(DateTime.UtcNow);

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyEmailCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenEmailAlreadyVerified()
    {
        var user = User.CreateTenantUser(_tenantId, "diana@example.com", "hashed", "Diana", "Prince");
        user.MarkEmailVerified();
        var token = EmailVerificationToken.Create(user.Id, "token-hash", DateTime.UtcNow.AddDays(3));
        token.Consume(DateTime.UtcNow);

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(token);
        await _dbContext.SaveChangesAsync();

        var command = new VerifyEmailCommand("raw-token");
        _tokenService.HashToken("raw-token").Returns("token-hash");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
