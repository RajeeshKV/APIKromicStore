namespace KromicStore.Domain.Shopping.ValueObjects;

/// <summary>
/// Money value object representing a monetary amount with currency.
/// Ensures type safety and prevents invalid money operations.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a valid ISO 4217 code (3 characters)", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Create a Money value object.
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Create zero money for a given currency.
    /// </summary>
    public static Money Zero(string currency)
    {
        return new Money(0, currency);
    }

    /// <summary>
    /// Add two Money values (currencies must match).
    /// </summary>
    public Money Add(Money other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtract one Money from another (currencies must match).
    /// </summary>
    public Money Subtract(Money other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiply Money by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Multiplication factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Check if amount is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Check if amount is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Check if amount is negative.
    /// </summary>
    public bool IsNegative => Amount < 0;

    public bool Equals(Money? other) =>
        other is not null &&
        Amount == other.Amount &&
        Currency == other.Currency;

    public override bool Equals(object? obj) =>
        obj is Money money && Equals(money);

    public override int GetHashCode() =>
        HashCode.Combine(Amount, Currency);

    public override string ToString() =>
        $"{Amount:N2} {Currency}";

    public static implicit operator decimal(Money money) =>
        money.Amount;

    public static bool operator ==(Money? left, Money? right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(Money? left, Money? right) =>
        !(left == right);

    public static bool operator <(Money? left, Money? right) =>
        left is not null && right is not null &&
        left.Currency == right.Currency &&
        left.Amount < right.Amount;

    public static bool operator <=(Money? left, Money? right) =>
        (left == right) || (left < right);

    public static bool operator >(Money? left, Money? right) =>
        left is not null && right is not null &&
        left.Currency == right.Currency &&
        left.Amount > right.Amount;

    public static bool operator >=(Money? left, Money? right) =>
        (left == right) || (left > right);
}
