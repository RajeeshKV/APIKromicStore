using KromicStore.Domain.Common;

namespace KromicStore.Domain.Identity;

public sealed class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresOnUtc, string? deviceName, string? ipAddress) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresOnUtc = EnsureUtc(expiresOnUtc);
        CreatedOnUtc = DateTime.UtcNow;
        DeviceName = deviceName;
        IPAddress = ipAddress;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresOnUtc { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? RevokedOnUtc { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IPAddress { get; private set; }
    public bool IsRevoked => RevokedOnUtc.HasValue;
    public bool IsExpired(DateTime utcNow) => ExpiresOnUtc <= EnsureUtc(utcNow);

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresOnUtc, string? deviceName, string? ipAddress)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Refresh token hash is required.", nameof(tokenHash));
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresOnUtc, deviceName, ipAddress);
    }

    public void Revoke(DateTime utcNow)
    {
        if (IsRevoked) return;
        RevokedOnUtc = EnsureUtc(utcNow);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
