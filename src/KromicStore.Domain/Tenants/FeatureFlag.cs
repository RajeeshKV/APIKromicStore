using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Feature flag for controlling platform features dynamically.
/// Can be enabled/disabled globally or per-subscription-plan or per-tenant.
/// </summary>
public sealed class FeatureFlag : AuditableEntity
{
    private readonly List<FeatureFlagAssignment> _assignments = [];

    private FeatureFlag()
    {
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    private FeatureFlag(Guid id, string code, string name, string description, bool isEnabled)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
    }

    // Identification
    public string Code { get; private set; } // e.g., "advanced_analytics", "custom_domains"
    public string Name { get; private set; }
    public string Description { get; private set; }

    // Status
    public bool IsEnabled { get; private set; }

    // Scope
    public FeatureFlagScope Scope { get; private set; } = FeatureFlagScope.Global;

    // Configuration
    public string? ConfigurationJson { get; private set; }

    // Assignments
    public IReadOnlyList<FeatureFlagAssignment> Assignments => _assignments.AsReadOnly();

    public static FeatureFlag Create(
        string code,
        string name,
        string description,
        bool isEnabled = true,
        FeatureFlagScope scope = FeatureFlagScope.Global)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new FeatureFlag(Guid.NewGuid(), code.Trim().ToUpperInvariant(), name.Trim(), description.Trim(), isEnabled)
        {
            Scope = scope
        };
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;

    public void SetConfiguration(string configurationJson)
    {
        ConfigurationJson = configurationJson;
    }

    public void AddAssignment(FeatureFlagAssignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        // Remove existing assignment for same entity
        _assignments.RemoveAll(a =>
            a.AssignmentType == assignment.AssignmentType &&
            a.AssignedToEntityId == assignment.AssignedToEntityId);

        _assignments.Add(assignment);
    }

    public void RemoveAssignment(Guid assignmentId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment != null)
            _assignments.Remove(assignment);
    }

    public bool IsEnabledFor(Guid? tenantId = null, Guid? planId = null)
    {
        if (!IsEnabled)
            return false;

        // Global flag enabled
        if (Scope == FeatureFlagScope.Global)
            return true;

        // Check specific assignments
        if (tenantId.HasValue)
        {
            var tenantAssignment = _assignments.FirstOrDefault(a =>
                a.AssignmentType == FeatureFlagAssignmentType.Tenant &&
                a.AssignedToEntityId == tenantId.Value);

            if (tenantAssignment != null)
                return tenantAssignment.IsEnabled;
        }

        if (planId.HasValue)
        {
            var planAssignment = _assignments.FirstOrDefault(a =>
                a.AssignmentType == FeatureFlagAssignmentType.SubscriptionPlan &&
                a.AssignedToEntityId == planId.Value);

            if (planAssignment != null)
                return planAssignment.IsEnabled;
        }

        return false;
    }
}

public enum FeatureFlagScope
{
    Global = 0,
    PerSubscriptionPlan = 1,
    PerTenant = 2
}

/// <summary>
/// Assignment of a feature flag to a subscription plan or tenant.
/// </summary>
public sealed class FeatureFlagAssignment : BaseEntity
{
    private FeatureFlagAssignment()
    {
    }

    private FeatureFlagAssignment(
        Guid id,
        Guid featureFlagId,
        FeatureFlagAssignmentType assignmentType,
        Guid assignedToEntityId,
        bool isEnabled)
        : base(id)
    {
        FeatureFlagId = featureFlagId;
        AssignmentType = assignmentType;
        AssignedToEntityId = assignedToEntityId;
        IsEnabled = isEnabled;
    }

    public Guid FeatureFlagId { get; private set; }
    public FeatureFlagAssignmentType AssignmentType { get; private set; }
    public Guid AssignedToEntityId { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime AssignedOnUtc { get; private set; } = DateTime.UtcNow;

    public static FeatureFlagAssignment Create(
        Guid featureFlagId,
        FeatureFlagAssignmentType assignmentType,
        Guid assignedToEntityId,
        bool isEnabled = true)
    {
        if (featureFlagId == Guid.Empty)
            throw new ArgumentException("FeatureFlagId is required.", nameof(featureFlagId));
        if (assignedToEntityId == Guid.Empty)
            throw new ArgumentException("AssignedToEntityId is required.", nameof(assignedToEntityId));

        return new FeatureFlagAssignment(Guid.NewGuid(), featureFlagId, assignmentType, assignedToEntityId, isEnabled);
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
}

public enum FeatureFlagAssignmentType
{
    SubscriptionPlan = 0,
    Tenant = 1
}
