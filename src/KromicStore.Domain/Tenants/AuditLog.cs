using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Platform audit log for tracking all administrative actions.
/// Used for compliance, security, and troubleshooting.
/// </summary>
public sealed class AuditLog : BaseEntity
{
    private AuditLog()
    {
        Action = string.Empty;
        EntityType = string.Empty;
    }

    private AuditLog(
        Guid id,
        string action,
        string entityType,
        Guid? entityId,
        Guid actorUserId,
        string? details)
        : base(id)
    {
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        ActorUserId = actorUserId;
        Details = details;
        OccurredOnUtc = DateTime.UtcNow;
    }

    // What happened
    public string Action { get; private set; } // Create, Update, Delete, Activate, Suspend, etc.
    public string EntityType { get; private set; } // Tenant, Plan, Theme, Settings, etc.
    public Guid? EntityId { get; private set; }
    public string? EntityName { get; private set; }

    // Who did it
    public Guid ActorUserId { get; private set; }
    public string? ActorEmail { get; private set; }

    // When
    public DateTime OccurredOnUtc { get; private set; }

    // Details
    public string? Details { get; private set; }
    public string? OldValues { get; private set; } // JSON
    public string? NewValues { get; private set; } // JSON
    public string? CorrelationId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // Severity
    public AuditLogSeverity Severity { get; private set; } = AuditLogSeverity.Info;

    // Status
    public bool IsSearchIndexed { get; private set; }

    public static AuditLog Create(
        string action,
        string entityType,
        Guid? entityId,
        Guid actorUserId,
        string? entityName = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        AuditLogSeverity severity = AuditLogSeverity.Info)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType is required.", nameof(entityType));
        if (actorUserId == Guid.Empty)
            throw new ArgumentException("ActorUserId is required.", nameof(actorUserId));

        var log = new AuditLog(Guid.NewGuid(), action.Trim(), entityType.Trim(), entityId, actorUserId, details)
        {
            EntityName = entityName,
            OldValues = oldValues,
            NewValues = newValues,
            Severity = severity
        };

        return log;
    }

    public void SetActorDetails(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
            ActorEmail = email.Trim();
    }

    public void SetContext(string? correlationId, string? ipAddress, string? userAgent)
    {
        CorrelationId = correlationId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public void MarkAsSearchIndexed() => IsSearchIndexed = true;
}

public enum AuditLogSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

/// <summary>
/// Well-known audit log actions.
/// </summary>
public static class AuditLogActions
{
    // Tenant
    public const string TenantCreated = "TenantCreated";
    public const string TenantUpdated = "TenantUpdated";
    public const string TenantActivated = "TenantActivated";
    public const string TenantSuspended = "TenantSuspended";
    public const string TenantArchived = "TenantArchived";
    public const string TenantRestored = "TenantRestored";
    public const string TenantDeleted = "TenantDeleted";

    // Subscription
    public const string SubscriptionPlanCreated = "SubscriptionPlanCreated";
    public const string SubscriptionPlanUpdated = "SubscriptionPlanUpdated";
    public const string SubscriptionPlanActivated = "SubscriptionPlanActivated";
    public const string SubscriptionPlanDeactivated = "SubscriptionPlanDeactivated";
    public const string SubscriptionPlanArchived = "SubscriptionPlanArchived";
    public const string TenantPlanAssigned = "TenantPlanAssigned";
    public const string TenantPlanUpgraded = "TenantPlanUpgraded";
    public const string TenantPlanDowngraded = "TenantPlanDowngraded";

    // Theme
    public const string ThemeCreated = "ThemeCreated";
    public const string ThemeUpdated = "ThemeUpdated";
    public const string ThemePublished = "ThemePublished";
    public const string ThemeUnpublished = "ThemeUnpublished";
    public const string ThemeArchived = "ThemeArchived";
    public const string ThemeDeleted = "ThemeDeleted";

    // Settings
    public const string PlatformSettingsUpdated = "PlatformSettingsUpdated";
    public const string FeatureFlagUpdated = "FeatureFlagUpdated";

    // Contact
    public const string ContactRequestReceived = "ContactRequestReceived";
    public const string ContactRequestResolved = "ContactRequestResolved";

    // Security
    public const string AdminLoginAttempted = "AdminLoginAttempted";
    public const string AdminLoginSucceeded = "AdminLoginSucceeded";
    public const string AdminLoginFailed = "AdminLoginFailed";
    public const string AdminActionUnauthorized = "AdminActionUnauthorized";
}
