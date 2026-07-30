using KromicStore.Domain.Identity;

namespace KromicStore.Domain.Tests.Identity;

public sealed class UserRoleTests
{
    [Fact]
    public void Create_ShouldSetUserIdAndRoleId()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = UserRole.Create(userId, roleId);

        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var act = () => UserRole.Create(Guid.Empty, Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithMessage("*UserId*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenRoleIdIsEmpty()
    {
        var act = () => UserRole.Create(Guid.NewGuid(), Guid.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("*RoleId*");
    }
}
