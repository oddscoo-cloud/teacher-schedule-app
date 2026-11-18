namespace backend.Presentation.Controllers;

using backend.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public abstract class AuthorizedWorkspaceControllerBase : ControllerBase
{
    protected bool TryGetRequester(out WorkspaceRequester requester)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleValue = User.FindFirstValue(ClaimTypes.Role);
        var membershipValue = User.FindFirstValue("workspaceMembershipId");

        if (int.TryParse(userIdValue, out var userId) &&
            Enum.TryParse<UserRole>(roleValue, out var role) &&
            int.TryParse(membershipValue, out var membershipId))
        {
            requester = new WorkspaceRequester(userId, role, membershipId);
            return true;
        }

        requester = default;
        return false;
    }
}

public readonly record struct WorkspaceRequester(int UserId, UserRole Role, int MembershipId);
