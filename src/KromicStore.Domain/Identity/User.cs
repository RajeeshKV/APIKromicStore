using KromicStore.Domain.Common;

namespace KromicStore.Domain.Identity;

public sealed class User : AuditableEntity
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<UserRole> _userRoles = [];

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private User(Guid id, Guid? tenantId, string email, string passwordHash, string firstName, string lastName) : base(id)
    {
        TenantId = tenantId;
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        IsActive = true;
        TokenVersion = 1;
    }

    public Guid? TenantId { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public int TokenVersion { get; private set; }
    public DateTime? LastLoginOnUtc { get; private set; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static User CreateTenantUser(Guid tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        return Create(tenantId, email, passwordHash, firstName, lastName);
    }

    public static User CreateSuperUser(string email, string passwordHash, string firstName, string lastName) =>
        Create(null, email, passwordHash, firstName, lastName);

    public void MarkEmailVerified() => IsEmailVerified = true;

    public void RecordLogin(DateTime utcNow) => LastLoginOnUtc = utcNow.Kind == DateTimeKind.Utc ? utcNow : DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public void Deactivate()
    {
        IsActive = false;
        TokenVersion++;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHash = passwordHash;
        TokenVersion++;
    }

    private static User Create(Guid? tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        return new User(Guid.NewGuid(), tenantId, email, passwordHash, firstName, lastName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
