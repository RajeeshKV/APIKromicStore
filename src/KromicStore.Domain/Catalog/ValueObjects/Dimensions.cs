namespace KromicStore.Domain.Catalog.ValueObjects;

/// <summary>
/// Value object representing product dimensions (length, width, height, weight).
/// All measurements are in metric units: cm for length/width/height, kg for weight.
/// </summary>
public sealed class Dimensions : IEquatable<Dimensions>
{
    public decimal? Length { get; } // cm
    public decimal? Width { get; }  // cm
    public decimal? Height { get; } // cm
    public decimal? Weight { get; } // kg

    private Dimensions(decimal? length, decimal? width, decimal? height, decimal? weight)
    {
        if (length.HasValue && length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));

        if (width.HasValue && width <= 0)
            throw new ArgumentException("Width must be greater than 0", nameof(width));

        if (height.HasValue && height <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(height));

        if (weight.HasValue && weight <= 0)
            throw new ArgumentException("Weight must be greater than 0", nameof(weight));

        Length = length;
        Width = width;
        Height = height;
        Weight = weight;
    }

    public static Dimensions Create(decimal? length = null, decimal? width = null, decimal? height = null, decimal? weight = null)
    {
        return new Dimensions(length, width, height, weight);
    }

    public static Dimensions Empty => new(null, null, null, null);

    public bool HasDimensions => Length.HasValue || Width.HasValue || Height.HasValue;
    public bool HasWeight => Weight.HasValue;

    public bool Equals(Dimensions? other) =>
        other is not null &&
        Length == other.Length &&
        Width == other.Width &&
        Height == other.Height &&
        Weight == other.Weight;

    public override bool Equals(object? obj) => obj is Dimensions dimensions && Equals(dimensions);

    public override int GetHashCode() =>
        HashCode.Combine(Length, Width, Height, Weight);

    public override string ToString()
    {
        var parts = new List<string>();

        if (Length.HasValue)
            parts.Add($"L: {Length}cm");
        if (Width.HasValue)
            parts.Add($"W: {Width}cm");
        if (Height.HasValue)
            parts.Add($"H: {Height}cm");
        if (Weight.HasValue)
            parts.Add($"Wt: {Weight}kg");

        return parts.Any() ? string.Join(", ", parts) : "No dimensions";
    }
}
