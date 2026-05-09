using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserBlog.Auth.JWT;
using UserBlog.Common.Constants;
using UserBlog.Common.Exceptions;
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

    public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.RefreshTokenSecret))
        {
            throw new InvalidOperationException("JWT refresh token secret is not configured.");
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.RefreshTokenSecret)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtSecurityToken)
            {
                throw new UnauthorizedAppException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid");
            }

            var algorithm = jwtSecurityToken.Header.Alg;

            if (!string.Equals(algorithm, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                throw new UnauthorizedAppException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid");
            }

            var tokenType = principal.FindFirstValue(JwtClaimNames.TokenType);

            if (tokenType != JwtClaimNames.RefreshTokenType)
            {
                throw new UnauthorizedAppException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid");
            }

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new UnauthorizedAppException(ErrorCodes.RefreshTokenExpired, "Refresh token has expired");
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAppException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid");
        }
        catch (ArgumentException)
        {
            throw new UnauthorizedAppException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid");
        }
    }

    private string GenerateAccessToken(User user, DateTimeOffset now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtClaimNames.Username, user.Username),
            new(JwtClaimNames.TokenType, JwtClaimNames.AccessTokenType)
        };

        return GenerateToken(claims, _jwtOptions.AccessTokenSecret, now, now.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes));
    }

    private string GenerateRefreshToken(User user, Guid refreshTokenId, DateTimeOffset now)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtClaimNames.TokenId, refreshTokenId.ToString()),
            new(JwtClaimNames.TokenType, JwtClaimNames.RefreshTokenType)
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