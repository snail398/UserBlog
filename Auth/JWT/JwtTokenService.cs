using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserBlog.Data.Entities;

namespace UserBlog.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public TokenPair GenerateTokenPair(User user)
    {
        var refreshTokenId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var accessToken = GenerateAccessToken(user, now);
        var refreshToken = GenerateRefreshToken(user, refreshTokenId, now);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenId = refreshTokenId,
            RefreshTokenExpiresAt = now.AddDays(_jwtOptions.RefreshTokenLifetimeDays)
        };
    }

    private string GenerateAccessToken(User user, DateTimeOffset now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.Username),
            new("type", "access")
        };

        return GenerateToken(claims, _jwtOptions.AccessTokenSecret, now, now.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes));
    }

    private string GenerateRefreshToken(User user, Guid refreshTokenId, DateTimeOffset now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tokenId", refreshTokenId.ToString()),
            new("type", "refresh")
        };

        return GenerateToken(claims, _jwtOptions.RefreshTokenSecret, now, now.AddDays(_jwtOptions.RefreshTokenLifetimeDays));
    }

    private string GenerateToken(IReadOnlyCollection<Claim> claims, string secret, DateTimeOffset now, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}