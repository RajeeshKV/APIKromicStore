using KromicStore.Domain.Common;

namespace KromicStore.Domain.StoreOperations.Entities;

/// <summary>
/// Inspection result for returned items.
/// </summary>
public enum InspectionResult
{
    Acceptable = 0,
    MinorDefects = 1,
    MajorDefects = 2,
    Unopened = 3,
    Wrong = 4
}

/// <summary>
/// ReturnInspection represents the inspection of returned items.
/// Determines if items are acceptable for restocking or must be discarded.
/// </summary>
public sealed class ReturnInspection : TenantEntity, IAuditable
{
    public Guid ReturnRequestId { get; private set; }
    public string InspectorNotes { get; private set; } = string.Empty;
    public InspectionResult Result { get; private set; }
    public DateTime InspectedOnUtc { get; private set; }
    public string InspectedBy { get; private set; } = string.Empty;
    
    // Restocking decision
    public bool IsRestockable { get; private set; }
    public decimal RestockableValue { get; private set; } // Amount to refund (may be less than original if damaged)
    public decimal WasteValue { get; private set; } // Amount written off
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    private ReturnInspection()
    {
    }
    
    private ReturnInspection(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new return inspection.
    /// </summary>
    public static ReturnInspection Create(
        Guid tenantId,
        Guid returnRequestId,
        InspectionResult result,
        string inspectorNotes,
        bool isRestockable,
        decimal restockableValue,
        decimal wasteValue,
        string inspectedBy)
    {
        if (returnRequestId == Guid.Empty)
            throw new ArgumentException("Return request ID is required", nameof(returnRequestId));
        
        if (string.IsNullOrWhiteSpace(inspectorNotes))
            throw new ArgumentException("Inspector notes are required", nameof(inspectorNotes));
        
        if (restockableValue < 0)
            throw new ArgumentException("Restockable value cannot be negative", nameof(restockableValue));
        
        if (wasteValue < 0)
            throw new ArgumentException("Waste value cannot be negative", nameof(wasteValue));
        
        if (restockableValue + wasteValue <= 0)
            throw new ArgumentException("Either restockable value or waste value must be greater than zero");
        
        if (string.IsNullOrWhiteSpace(inspectedBy))
            throw new ArgumentException("Inspected by is required", nameof(inspectedBy));
        
        var inspection = new ReturnInspection(Guid.NewGuid(), tenantId)
        {
            ReturnRequestId = returnRequestId,
            Result = result,
            InspectorNotes = inspectorNotes.Trim(),
            IsRestockable = isRestockable,
            RestockableValue = restockableValue,
            WasteValue = wasteValue,
            InspectedOnUtc = DateTime.UtcNow,
            InspectedBy = inspectedBy.Trim(),
            CreatedBy = inspectedBy.Trim()
        };
        
        return inspection;
    }
    
    /// <summary>
    /// Get the total inspection value (restockable + waste).
    /// </summary>
    public decimal GetTotalInspectionValue() => RestockableValue + WasteValue;
    
    /// <summary>
    /// Determine if items are acceptable for restocking.
    /// </summary>
    public bool CanBeRestocked() => Result == InspectionResult.Acceptable ||
                                   Result == InspectionResult.Unopened ||
                                   Result == InspectionResult.MinorDefects;
    
    /// <summary>
    /// Update inspection details.
    /// </summary>
    public void UpdateInspection(
        InspectionResult result,
        string notes,
        bool isRestockable,
        decimal restockableValue,
        decimal wasteValue)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes are required", nameof(notes));
        
        if (restockableValue < 0)
            throw new ArgumentException("Restockable value cannot be negative", nameof(restockableValue));
        
        if (wasteValue < 0)
            throw new ArgumentException("Waste value cannot be negative", nameof(wasteValue));
        
        if (restockableValue + wasteValue <= 0)
            throw new ArgumentException("Either restockable value or waste value must be greater than zero");
        
        Result = result;
        InspectorNotes = notes.Trim();
        IsRestockable = isRestockable;
        RestockableValue = restockableValue;
        WasteValue = wasteValue;
    }
}
