using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Application.Features.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Loads the authenticated user's full profile including resolved role names.
/// Handlers never return raw EF entities — only DTOs.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService   _currentUser;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService   currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<UserProfileResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken   cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User", userId);

        var roleIds   = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roleNames = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        return new UserProfileResponse(
            Id:              user.Id,
            TenantId:        user.TenantId,
            Email:           user.Email,
            FirstName:       user.FirstName,
            LastName:        user.LastName,
            IsEmailVerified: user.IsEmailVerified,
            Roles:           roleNames);
    }
}
