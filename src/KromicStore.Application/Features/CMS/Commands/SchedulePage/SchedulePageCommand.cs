using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.SchedulePage;

/// <summary>
/// Command to schedule a CMS page for future publication.
/// </summary>
public sealed record SchedulePageCommand(
    Guid PageId,
    Guid TenantId,
    DateTime PublishDateUtc) : IRequest<SchedulePageResponse>;

/// <summary>
/// Response from SchedulePageCommand.
/// </summary>
public sealed record SchedulePageResponse(
    Guid PageId,
    string Status,
    DateTime? ScheduledPublishDateUtc);
