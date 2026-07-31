namespace KromicStore.API.Contracts.ThemeBuilder;

/// <summary>
/// Request to create a new theme.
/// </summary>
public class CreateThemeRequest
{
    /// <summary>
    /// Theme name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Theme description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base template to start from.
    /// </summary>
    public string BaseTemplate { get; set; } = "Minimal";
}
