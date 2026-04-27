using System.Security.Claims;
using UserBlog.Common.Exceptions;

namespace UserBlog.Common;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                throw new UnauthorizedAppException("UNAUTHORIZED", "Current user is not authenticated");
            }

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAppException("INVALID_ACCESS_TOKEN", "Access token contains invalid user id");
            }

            return userId;
        }
    }
}