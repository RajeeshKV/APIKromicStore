using KromicStore.Domain.Common;

namespace KromicStore.Domain.Identity;

public sealed class EmailVerificationToken : BaseEntity
{
    private EmailVerificationToken()
    {
        TokenHash = string.Empty;
    }

    private EmailVerificationToken(Guid id, Guid userId, string tokenHash, DateTime expiresOnUtc) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresOnUtc = EnsureUtc(expiresOnUtc);
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresOnUtc { get; private set; }
    public DateTime? ConsumedOnUtc { get; private set; }
    public bool IsConsumed => ConsumedOnUtc.HasValue;

    public static EmailVerificationToken Create(Guid userId, string tokenHash, DateTime expiresOnUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        return new EmailVerificationToken(Guid.NewGuid(), userId, tokenHash, expiresOnUtc);
    }

    public void Consume(DateTime utcNow)
    {
        if (IsConsumed) return;
        ConsumedOnUtc = EnsureUtc(utcNow);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
