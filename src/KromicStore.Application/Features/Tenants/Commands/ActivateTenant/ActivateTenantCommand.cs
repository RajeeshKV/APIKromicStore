using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.ActivateTenant;

/// <summary>
/// Command to activate a suspended or provisioning tenant.
/// </summary>
public sealed class ActivateTenantCommand : IRequest<ActivateTenantResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class ActivateTenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedOnUtc { get; set; }
}
