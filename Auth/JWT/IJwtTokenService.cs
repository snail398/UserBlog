using UserBlog.Data.Entities;

namespace UserBlog.Auth;

public interface IJwtTokenService
{
    TokenPair GenerateTokenPair(User user);
}