namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Configuration options for database operations during startup.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name for appsettings.json binding.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets whether to apply pending EF Core migrations automatically on startup.
    /// 
    /// Default: true (enabled for production safety)
    /// Set to false to disable automatic migration execution.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum timeout in seconds for migration execution.
    /// 
    /// Default: 300 seconds (5 minutes)
    /// Prevents migrations from hanging indefinitely.
    /// </summary>
    public int MigrationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets whether to continue startup if migrations fail.
    /// 
    /// Default: false (startup fails on migration error)
    /// Set to true only if you have alternative migration strategies.
    /// </summary>
    public bool ContinueOnMigrationFailure { get; set; } = false;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if configuration is invalid.</exception>
    public void Validate()
    {
        if (MigrationTimeoutSeconds < 30)
        {
            throw new ArgumentException(
                $"{nameof(MigrationTimeoutSeconds)} must be at least 30 seconds, got {MigrationTimeoutSeconds}",
                nameof(MigrationTimeoutSeconds));
        }

        if (MigrationTimeoutSeconds > 3600)
        {
            throw new ArgumentException(
                $"{nameof(MigrationTimeoutSeconds)} must not exceed 3600 seconds, got {MigrationTimeoutSeconds}",
                nameof(MigrationTimeoutSeconds));
        }
    }
}
