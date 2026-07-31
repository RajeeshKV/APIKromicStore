namespace KromicStore.API.Contracts.ThemeBuilder;

/// <summary>
/// Request to update an existing theme.
/// </summary>
public class UpdateThemeRequest
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
    /// Theme custom CSS or styling.
    /// </summary>
    public string? CustomCSS { get; set; }

    /// <summary>
    /// Color scheme configuration.
    /// </summary>
    public Dictionary<string, string>? ColorScheme { get; set; }
}
