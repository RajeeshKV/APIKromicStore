using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.ArchiveTenant;

public sealed class ArchiveTenantCommand : IRequest<ArchiveTenantResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class ArchiveTenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
