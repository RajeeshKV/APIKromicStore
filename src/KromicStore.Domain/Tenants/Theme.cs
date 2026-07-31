using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Public theme managed by the platform.
/// Themes can be used by tenants to customize their storefronts.
/// </summary>
public sealed class Theme : AuditableEntity
{
    private readonly List<ThemeVersion> _versions = [];

    private Theme()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    private Theme(Guid id, string name, string slug, string? description)
        : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        Status = ThemeStatus.Draft;
    }

    // Identification
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }

    // Media
    public string? PreviewImageUrl { get; private set; }
    public string? ThumbnailImageUrl { get; private set; }

    // Configuration
    public string? ConfigurationSchema { get; private set; } // JSON schema for theme settings
    public string? DefaultConfiguration { get; private set; } // JSON default values

    // Status
    public ThemeStatus Status { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime? PublishedOnUtc { get; private set; }

    // Usage
    public int TimesUsed { get; private set; }

    // Versioning
    public int CurrentVersion { get; private set; } = 1;
    public IReadOnlyList<ThemeVersion> Versions => _versions.AsReadOnly();

    public static Theme Create(
        string name,
        string slug,
        string? description = null,
        string? previewImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required.", nameof(slug));

        var theme = new Theme(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(), description?.Trim())
        {
            PreviewImageUrl = previewImageUrl
        };

        // Create initial version
        theme._versions.Add(ThemeVersion.Create(theme.Id, 1, "Initial"));

        return theme;
    }

    public void Update(
        string name,
        string? description,
        string? previewImageUrl,
        string? thumbnailImageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        PreviewImageUrl = previewImageUrl;
        ThumbnailImageUrl = thumbnailImageUrl;
    }

    public void SetConfiguration(string configSchema, string defaultConfig)
    {
        ConfigurationSchema = configSchema;
        DefaultConfiguration = defaultConfig;
    }

    public void Publish()
    {
        if (Status == ThemeStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived theme.");

        Status = ThemeStatus.Published;
        IsPublished = true;
        PublishedOnUtc = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        Status = ThemeStatus.Draft;
        IsPublished = false;
    }

    public void Archive()
    {
        Status = ThemeStatus.Archived;
        IsPublished = false;
    }

    public void Restore()
    {
        Status = ThemeStatus.Draft;
    }

    public void IncreaseUsageCount()
    {
        TimesUsed++;
    }

    public void DecreaseUsageCount()
    {
        if (TimesUsed > 0)
            TimesUsed--;
    }

    public ThemeVersion CreateNewVersion(string changesSummary)
    {
        if (Status == ThemeStatus.Archived)
            throw new InvalidOperationException("Cannot create a version for an archived theme.");

        CurrentVersion++;
        var newVersion = ThemeVersion.Create(Id, CurrentVersion, changesSummary);
        _versions.Add(newVersion);
        return newVersion;
    }

    public ThemeVersion? Clone(string newName, string newSlug)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New theme name is required.", nameof(newName));

        // Note: Actual cloning creates a new Theme entity; this just clones a version
        var version = _versions.LastOrDefault();
        return version?.Clone(newName);
    }
}

public enum ThemeStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

/// <summary>
/// Version of a theme for tracking changes and rollbacks.
/// </summary>
public sealed class ThemeVersion : BaseEntity
{
    private ThemeVersion()
    {
        Content = string.Empty;
        ChangesSummary = string.Empty;
    }

    private ThemeVersion(Guid id, Guid themeId, int versionNumber, string changesSummary)
        : base(id)
    {
        ThemeId = themeId;
        VersionNumber = versionNumber;
        ChangesSummary = changesSummary;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public Guid ThemeId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Content { get; private set; } = string.Empty; // The actual theme files/code
    public string ChangesSummary { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static ThemeVersion Create(Guid themeId, int versionNumber, string changesSummary)
    {
        if (themeId == Guid.Empty)
            throw new ArgumentException("ThemeId is required.", nameof(themeId));
        if (string.IsNullOrWhiteSpace(changesSummary))
            throw new ArgumentException("ChangesSummary is required.", nameof(changesSummary));

        return new ThemeVersion(Guid.NewGuid(), themeId, versionNumber, changesSummary.Trim());
    }

    public void SetContent(string content)
    {
        Content = content;
    }

    public void Activate() => IsActive = true;

    public ThemeVersion Clone(string newThemeName)
    {
        return new ThemeVersion(Guid.NewGuid(), ThemeId, VersionNumber, $"Cloned from {newThemeName}")
        {
            Content = Content,
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
