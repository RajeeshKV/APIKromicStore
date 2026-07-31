namespace KromicStore.API.Contracts.ThemeBuilder;

/// <summary>
/// DTO representing a theme version in history.
/// </summary>
public class ThemeVersionDto
{
    /// <summary>
    /// Version number.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Version description/change notes.
    /// </summary>
    public string? ChangeNotes { get; set; }

    /// <summary>
    /// When version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether this version is currently published.
    /// </summary>
    public bool IsPublished { get; set; }
}
