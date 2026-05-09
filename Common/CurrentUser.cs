using System.Security.Claims;
using UserBlog.Auth.JWT;
using UserBlog.Common.Constants;
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

            var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue(JwtClaimNames.Subject);

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                throw new UnauthorizedAppException(ErrorCodes.Unauthorized, "Current user is not authenticated");
            }

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAppException(ErrorCodes.InvalidAccessToken, "Access token contains invalid user id");
            }

            return userId;
        }
    }

    public bool TryGetUserId(out Guid userId)
    {
        userId = default;

        var user = _httpContextAccessor.HttpContext?.User;

        var userIdValue = user?.FindFirstValue(JwtClaimNames.Subject);

        return Guid.TryParse(userIdValue, out userId);
    }
}