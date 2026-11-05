using System.Security.Claims;
using DndJornada.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DndJornada.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected UserRole? CurrentUserRole
    {
        get
        {
            var roleStr = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleStr, out var role) ? role : null;
        }
    }
}

