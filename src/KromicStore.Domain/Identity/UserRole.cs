namespace KromicStore.Domain.Identity;

public sealed class UserRole
{
    private UserRole()
    {
    }

    private UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (roleId == Guid.Empty) throw new ArgumentException("RoleId is required.", nameof(roleId));
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public static UserRole Create(Guid userId, Guid roleId) => new(userId, roleId);
}
