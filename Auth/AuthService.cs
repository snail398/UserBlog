using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserBlog.Auth.Dtos;
using UserBlog.Common.Exceptions;
using UserBlog.Data;
using UserBlog.Data.Entities;
using UserBlog.Users;
using UserBlog.Users.Dtos;

namespace UserBlog.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenHasher _tokenHasher;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, ITokenHasher tokenHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _tokenHasher = tokenHasher;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var username = request.Username.Trim();

        await EnsureUserDoesNotExistAsync(email, username, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "ux_users_email"))
        {
            throw new ConflictException("EMAIL_ALREADY_EXISTS", "User with this email already exists");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "ux_users_username"))
        {
            throw new ConflictException("USERNAME_ALREADY_EXISTS", "User with this username already exists");
        }

        return UserMapper.ToResponse(user);
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName == constraintName;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("INVALID_CREDENTIALS", "Invalid email or password");
        }

        var tokenPair = _jwtTokenService.GenerateTokenPair(user);

        var refreshToken = new RefreshToken
        {
            Id = tokenPair.RefreshTokenId,
            UserId = user.Id,
            TokenHash = _tokenHasher.Hash(tokenPair.RefreshToken),
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            RevokedAt = null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            User = UserMapper.ToResponse(user)
        };
    }
    
    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);

        var userId = GetUserIdFromPrincipal(principal);
        var refreshTokenId = GetRefreshTokenIdFromPrincipal(principal);

        var storedRefreshToken = await _dbContext.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken);

        if (storedRefreshToken is null)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        if (storedRefreshToken.UserId != userId)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        if (storedRefreshToken.RevokedAt is not null)
        {
            throw new UnauthorizedAppException("REFRESH_TOKEN_REVOKED", "Refresh token has been revoked");
        }

        if (storedRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAppException("REFRESH_TOKEN_EXPIRED", "Refresh token has expired");
        }

        var refreshTokenHash = _tokenHasher.Hash(request.RefreshToken);

        if (!string.Equals(storedRefreshToken.TokenHash, refreshTokenHash, StringComparison.Ordinal))
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        var user = storedRefreshToken.User;

        var newTokenPair = _jwtTokenService.GenerateTokenPair(user);

        storedRefreshToken.RevokedAt = DateTimeOffset.UtcNow;

        var newRefreshToken = new RefreshToken
        {
            Id = newTokenPair.RefreshTokenId,
            UserId = user.Id,
            TokenHash = _tokenHasher.Hash(newTokenPair.RefreshToken),
            ExpiresAt = newTokenPair.RefreshTokenExpiresAt,
            RevokedAt = null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = newTokenPair.AccessToken,
            RefreshToken = newTokenPair.RefreshToken,
            User = UserMapper.ToResponse(user)
        };
    }

    public async Task<LogoutResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);

        var userId = GetUserIdFromPrincipal(principal);
        var refreshTokenId = GetRefreshTokenIdFromPrincipal(principal);

        var storedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken);

        if (storedRefreshToken is null)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        if (storedRefreshToken.UserId != userId)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        var refreshTokenHash = _tokenHasher.Hash(request.RefreshToken);

        if (!string.Equals(storedRefreshToken.TokenHash, refreshTokenHash, StringComparison.Ordinal))
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token is invalid");
        }

        if (storedRefreshToken.RevokedAt is null)
        {
            storedRefreshToken.RevokedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new LogoutResponse
        {
            Success = true
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private async Task EnsureUserDoesNotExistAsync(string email, string username, CancellationToken cancellationToken)
    {
        var existingUsers = await _dbContext.Users
            .Where(x => x.Email == email || x.Username == username)
            .Select(x => new {x.Email, x.Username})
            .ToListAsync(cancellationToken);

        var emailExists = existingUsers.Any(x => x.Email == email);
        var usernameExists = existingUsers.Any(x => x.Username == username);

        if (emailExists)
        {
            throw new ConflictException("EMAIL_ALREADY_EXISTS", "User with this email already exists");
        }

        if (usernameExists)
        {
            throw new ConflictException("USERNAME_ALREADY_EXISTS", "User with this username already exists");
        }
    }

    private static Guid GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        var userIdValue = principal.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token contains invalid user id");
        }

        return userId;
    }

    private static Guid GetRefreshTokenIdFromPrincipal(ClaimsPrincipal principal)
    {
        var tokenIdValue = principal.FindFirstValue("tokenId");

        if (!Guid.TryParse(tokenIdValue, out var tokenId))
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "Refresh token contains invalid token id");
        }

        return tokenId;
    }
}