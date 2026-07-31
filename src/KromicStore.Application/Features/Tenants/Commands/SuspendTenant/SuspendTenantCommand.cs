using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.SuspendTenant;

/// <summary>
/// Command to suspend an active tenant.
/// </summary>
public sealed class SuspendTenantCommand : IRequest<SuspendTenantResponse>
{
    public Guid TenantId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class SuspendTenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedOnUtc { get; set; }
}
