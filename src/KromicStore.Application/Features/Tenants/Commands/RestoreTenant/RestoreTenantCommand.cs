using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.RestoreTenant;

public sealed class RestoreTenantCommand : IRequest<RestoreTenantResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class RestoreTenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
