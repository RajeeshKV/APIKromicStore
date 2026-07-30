using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.ForgotPassword;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly ForgotPasswordCommandHandler _sut;

    public ForgotPasswordCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<ForgotPasswordCommandHandler>>();
        _sut = new ForgotPasswordCommandHandler(_dbContext, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateResetToken_WhenUserExists()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        var command = new ForgotPasswordCommand("alice@example.com");
        _tokenService.GenerateRefreshToken().Returns("reset-token");
        _tokenService.HashToken("reset-token").Returns("reset-token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var resetTokens = _dbContext.PasswordResetTokens.Where(t => t.UserId == user.Id).ToList();
        resetTokens.Should().HaveCount(1);
        resetTokens[0].IsConsumed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldBeSilent_WhenUserNotFound()
    {
        var command = new ForgotPasswordCommand("nonexistent@example.com");

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldConsumeOldTokens_BeforeCreatingNew()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var oldToken1 = PasswordResetToken.Create(user.Id, "old-hash-1", DateTime.UtcNow.AddDays(1));
        var oldToken2 = PasswordResetToken.Create(user.Id, "old-hash-2", DateTime.UtcNow.AddDays(1));
        _dbContext.AddEntity(oldToken1);
        _dbContext.AddEntity(oldToken2);
        await _dbContext.SaveChangesAsync();

        var command = new ForgotPasswordCommand("bob@example.com");
        _tokenService.GenerateRefreshToken().Returns("new-token");
        _tokenService.HashToken("new-token").Returns("new-token-hash");

        await _sut.Handle(command, CancellationToken.None);

        var consumedCount = _dbContext.PasswordResetTokens.Where(t => t.UserId == user.Id && t.ConsumedOnUtc.HasValue).Count();
        consumedCount.Should().Be(2);

        var activeTokenCount = _dbContext.PasswordResetTokens.Where(t => t.UserId == user.Id && !t.ConsumedOnUtc.HasValue).Count();
        activeTokenCount.Should().Be(1);
    }
}
