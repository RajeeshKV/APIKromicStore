using System.Text.RegularExpressions;

namespace KromicStore.Domain.Catalog.ValueObjects;

/// <summary>
/// Value object representing a Stock Keeping Unit (SKU).
/// SKUs are unique within a tenant and used for inventory tracking.
/// </summary>
public sealed class Sku : IEquatable<Sku>
{
    public string Value { get; }

    private Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length < 1 || trimmed.Length > 50)
            throw new ArgumentException("SKU must be between 1 and 50 characters", nameof(value));

        if (!IsValidSkuFormat(trimmed))
            throw new ArgumentException("SKU must contain only uppercase letters, numbers, and hyphens", nameof(value));

        Value = trimmed;
    }

    public static Sku Create(string value)
    {
        return new Sku(value);
    }

    private static bool IsValidSkuFormat(string sku)
    {
        return Regex.IsMatch(sku, @"^[A-Z0-9\-]+$");
    }

    public bool Equals(Sku? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Sku sku && Equals(sku);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Sku sku) => sku.Value;
    public static explicit operator Sku(string value) => new(value);
}
