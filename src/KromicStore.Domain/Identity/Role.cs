using KromicStore.Domain.Common;

namespace KromicStore.Domain.Identity;

public sealed class Role : AuditableEntity
{
    private Role()
    {
        Name = string.Empty;
    }

    private Role(Guid id, string name) : base(id)
    {
        Name = name.Trim();
    }

    public string Name { get; private set; }

    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));
        return new Role(Guid.NewGuid(), name);
    }
}
