namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Tracks the application initialization state.
/// Used by health checks to determine if the application is fully operational.
/// </summary>
public sealed class ApplicationStartupState
{
    private bool _isInitialized;
    private bool _isDependencyInjectionReady;
    private DateTime _startupTime;

    /// <summary>
    /// Gets whether the application initialization has completed.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Gets whether the Dependency Injection container is ready.
    /// </summary>
    public bool IsDependencyInjectionReady => _isDependencyInjectionReady;

    /// <summary>
    /// Gets the UTC timestamp when the application started.
    /// </summary>
    public DateTime StartupTime => _startupTime;

    /// <summary>
    /// Gets the time elapsed since application startup.
    /// </summary>
    public TimeSpan UpTime => DateTime.UtcNow - _startupTime;

    /// <summary>
    /// Marks the Dependency Injection container as ready.
    /// </summary>
    public void MarkDependencyInjectionReady()
    {
        _isDependencyInjectionReady = true;
    }

    /// <summary>
    /// Marks the application as fully initialized and ready to serve requests.
    /// </summary>
    public void MarkInitialized()
    {
        _startupTime = DateTime.UtcNow;
        _isInitialized = true;
    }

    /// <summary>
    /// Resets the startup state (useful for testing).
    /// </summary>
    public void Reset()
    {
        _isInitialized = false;
        _isDependencyInjectionReady = false;
        _startupTime = DateTime.MinValue;
    }
}
