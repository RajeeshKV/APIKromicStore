using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.SendPlatformNotification;

public sealed class SendPlatformNotificationCommand : IRequest<Unit>
{
    public string NotificationType { get; set; } = string.Empty; // announcement, maintenance, tenant_broadcast
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? TargetTenantId { get; set; }
    public bool SendEmail { get; set; } = true;
}
