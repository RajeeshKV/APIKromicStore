using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.ResendVerificationEmail;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class ResendVerificationEmailCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;
    private readonly ResendVerificationEmailCommandHandler _sut;

    public ResendVerificationEmailCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<ResendVerificationEmailCommandHandler>>();
        _sut = new ResendVerificationEmailCommandHandler(_dbContext, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldResendToken_WhenUserNotVerified()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new ResendVerificationEmailCommand("alice@example.com");
        _tokenService.GenerateRefreshToken().Returns("new-token");
        _tokenService.HashToken("new-token").Returns("new-token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var verificationTokens = _dbContext.EmailVerificationTokens.Where(t => t.UserId == user.Id).ToList();
        verificationTokens.Should().HaveCount(1);
        verificationTokens[0].IsConsumed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldBeSilent_WhenUserNotFound()
    {
        var command = new ResendVerificationEmailCommand("nonexistent@example.com");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldBeSilent_WhenEmailAlreadyVerified()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new ResendVerificationEmailCommand("bob@example.com");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldConsumePreviousToken_BeforeCreatingNew()
    {
        var user = User.CreateTenantUser(_tenantId, "charlie@example.com", "hashed", "Charlie", "Brown");
        var oldToken = EmailVerificationToken.Create(user.Id, "old-token-hash", DateTime.UtcNow.AddDays(3));

        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        _dbContext.AddEntity(oldToken);
        await _dbContext.SaveChangesAsync();

        var command = new ResendVerificationEmailCommand("charlie@example.com");
        _tokenService.GenerateRefreshToken().Returns("new-token");
        _tokenService.HashToken("new-token").Returns("new-token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var allTokens = _dbContext.EmailVerificationTokens.Where(t => t.UserId == user.Id).ToList();
        allTokens.Should().HaveCount(2);

        var oldStoredToken = allTokens.First(t => t.TokenHash == "old-token-hash");
        oldStoredToken.IsConsumed.Should().BeTrue();

        var newStoredToken = allTokens.First(t => t.TokenHash == "new-token-hash");
        newStoredToken.IsConsumed.Should().BeFalse();
    }
}
