using System.Security.Claims;
using UserBlog.Data.Entities;

namespace UserBlog.Auth;

public interface IJwtTokenService
{
    TokenPair GenerateTokenPair(User user);

    ClaimsPrincipal ValidateRefreshToken(string refreshToken);
}