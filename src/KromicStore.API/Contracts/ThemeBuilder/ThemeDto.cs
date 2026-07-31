namespace KromicStore.API.Contracts.ThemeBuilder;

/// <summary>
/// DTO representing a store theme.
/// </summary>
public class ThemeDto
{
    /// <summary>
    /// Unique theme identifier.
    /// </summary>
    public Guid ThemeId { get; set; }

    /// <summary>
    /// Theme name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Theme description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base template used (Minimal, Professional, E-commerce).
    /// </summary>
    public string BaseTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Whether theme is currently active on the store.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether theme has been published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Current version number.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// When theme was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When theme was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
