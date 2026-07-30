using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Commands.ChangePassword;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Tests.Features.Authentication.Commands;

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _logger = Substitute.For<ILogger<ChangePasswordCommandHandler>>();
        _sut = new ChangePasswordCommandHandler(_dbContext, _passwordHasher, _currentUserService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldChangePassword_WhenCurrentPasswordCorrect()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "old-hash", "Alice", "Smith");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");
        _passwordHasher.Verify("old-hash", "OldPass1!").Returns(true);
        _passwordHasher.Hash("NewPass1!").Returns("new-hash");

        await _sut.Handle(command, CancellationToken.None);

        var updatedUser = _dbContext.Users.First(u => u.Id == user.Id);
        updatedUser.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCurrentPasswordWrong()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "old-hash", "Bob", "Jones");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ChangePasswordCommand("WrongOldPass!", "NewPass1!", "NewPass1!");
        _passwordHasher.Verify("old-hash", "WrongOldPass!").Returns(false);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task Handle_ShouldRevokeAllRefreshTokens_ToForceRelogin()
    {
        var user = User.CreateTenantUser(_tenantId, "diana@example.com", "old-hash", "Diana", "Prince");
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);

        var token1 = DomainRefreshToken.Create(user.Id, "token-1", DateTime.UtcNow.AddDays(15), "iPhone", "192.168.1.1");
        var token2 = DomainRefreshToken.Create(user.Id, "token-2", DateTime.UtcNow.AddDays(15), "iPad", "192.168.1.2");
        _dbContext.AddEntity(token1);
        _dbContext.AddEntity(token2);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");
        _passwordHasher.Verify("old-hash", "OldPass1!").Returns(true);
        _passwordHasher.Hash("NewPass1!").Returns("new-hash");

        await _sut.Handle(command, CancellationToken.None);

        var allTokens = _dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToList();
        allTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }
}
