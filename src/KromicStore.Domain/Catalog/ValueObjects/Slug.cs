using System.Text.RegularExpressions;
using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.ValueObjects;

/// <summary>
/// Value object representing a URL-friendly slug.
/// Slugs are unique within a tenant and auto-generated from names.
/// </summary>
public sealed class Slug : IEquatable<Slug>
{
    public string Value { get; }

    private Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Slug cannot exceed 100 characters", nameof(value));

        if (!IsValidSlugFormat(value))
            throw new ArgumentException("Slug must contain only lowercase letters, numbers, and hyphens", nameof(value));

        Value = value;
    }

    public static Slug Create(string? customSlug, string name)
    {
        if (!string.IsNullOrWhiteSpace(customSlug))
        {
            return new Slug(customSlug.Trim().ToLowerInvariant());
        }

        var generated = GenerateFromName(name);
        return new Slug(generated);
    }

    public static string GenerateFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove any character that is not alphanumeric or hyphen
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Replace multiple consecutive hyphens with a single hyphen
        slug = Regex.Replace(slug, @"\-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Ensure we have a valid slug
        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Could not generate valid slug from name", nameof(name));

        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }

    private static bool IsValidSlugFormat(string slug)
    {
        return Regex.IsMatch(slug, @"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$");
    }

    public bool Equals(Slug? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Slug slug && Equals(slug);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Slug slug) => slug.Value;
    public static explicit operator Slug(string value) => new(value);
}
