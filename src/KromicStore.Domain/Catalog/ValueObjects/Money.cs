namespace KromicStore.Domain.Catalog.ValueObjects;

/// <summary>
/// Value object representing a monetary amount.
/// Ensures price consistency and proper decimal handling.
/// </summary>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters (ISO 4217)", nameof(currency));

        Amount = decimal.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        return new Money(amount, currency);
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public bool IsZero => Amount == 0m;
    public bool IsPositive => Amount > 0m;

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    public int CompareTo(Money? other)
    {
        if (other is null)
            return 1;

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare {other.Currency} with {Currency}");

        return Amount.CompareTo(other.Amount);
    }

    public bool Equals(Money? other) =>
        other is not null &&
        Amount == other.Amount &&
        Currency == other.Currency;

    public override bool Equals(object? obj) => obj is Money money && Equals(money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Currency} {Amount:F2}";

    public static bool operator ==(Money? left, Money? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Money? left, Money? right) =>
        !(left == right);

    public static bool operator <(Money left, Money right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(Money left, Money right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(Money left, Money right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(Money left, Money right) =>
        left.CompareTo(right) >= 0;
}
