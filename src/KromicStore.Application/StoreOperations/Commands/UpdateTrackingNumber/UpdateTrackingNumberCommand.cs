using MediatR;

namespace KromicStore.Application.StoreOperations.Commands.UpdateTrackingNumber;

/// <summary>
/// Command to update tracking information for a shipment.
/// </summary>
public sealed class UpdateTrackingNumberCommand : IRequest<UpdateTrackingNumberResponse>
{
    public Guid FulfillmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
}

public sealed class UpdateTrackingNumberResponse
{
    public Guid FulfillmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
    public DateTime UpdatedOnUtc { get; set; }
}
