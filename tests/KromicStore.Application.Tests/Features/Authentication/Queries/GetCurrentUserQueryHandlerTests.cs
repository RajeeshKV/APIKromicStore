using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.Queries.GetCurrentUser;
using KromicStore.Application.Tests.Common;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Persistence;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Authentication.Queries;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        var actualDb = InMemoryDbContextFactory.Create(_tenantId);
        _dbContext = actualDb;
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetCurrentUserQueryHandler(_dbContext, _currentUserService);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        var user = User.CreateTenantUser(_tenantId, "alice@example.com", "hashed", "Alice", "Smith");
        user.MarkEmailVerified();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("alice@example.com");
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
        result.IsEmailVerified.Should().BeTrue();
        result.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task Handle_ShouldMapRoles_Correctly()
    {
        var user = User.CreateTenantUser(_tenantId, "bob@example.com", "hashed", "Bob", "Jones");
        _dbContext.AddEntity(user);

        var role = Role.Create("Admin");
        _dbContext.AddEntity(role);

        var userRole = UserRole.Create(user.Id, role.Id);
        _dbContext.AddEntity(userRole);

        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var nonExistentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(nonExistentUserId);

        var query = new GetCurrentUserQuery();
        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldIncludeInactiveUser()
    {
        var user = User.CreateTenantUser(_tenantId, "charlie@example.com", "hashed", "Charlie", "Brown");
        user.MarkEmailVerified();
        user.Deactivate();
        ((KromicStoreDbContext)_dbContext).UserSet.Add(user);
        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleRoles()
    {
        var user = User.CreateTenantUser(_tenantId, "diana@example.com", "hashed", "Diana", "Prince");
        _dbContext.AddEntity(user);

        var adminRole = Role.Create("Admin");
        var editorRole = Role.Create("Editor");
        _dbContext.AddEntity(adminRole);
        _dbContext.AddEntity(editorRole);

        var userRole1 = UserRole.Create(user.Id, adminRole.Id);
        var userRole2 = UserRole.Create(user.Id, editorRole.Id);
        _dbContext.AddEntity(userRole1);
        _dbContext.AddEntity(userRole2);

        await _dbContext.SaveChangesAsync();

        _currentUserService.UserId.Returns(user.Id);

        var query = new GetCurrentUserQuery();
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Roles.Should().HaveCount(2);
        result.Roles.Should().Contain(new[] { "Admin", "Editor" });
    }
}
