using Microsoft.EntityFrameworkCore;
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
        ValidateRegisterRequest(request);

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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return UserMapper.ToResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLoginRequest(request);

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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = "Email is required";
        }
        else if (!IsValidEmail(request.Email))
        {
            errors["email"] = "Email is invalid";
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors["username"] = "Username is required";
        }
        else
        {
            var username = request.Username.Trim();

            if (username.Length < 3 || username.Length > 32)
            {
                errors["username"] = "Username must be between 3 and 32 characters";
            }
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = "Password is required";
        }
        else if (request.Password.Length < 8)
        {
            errors["password"] = "Password must be at least 8 characters";
        }
        else if (request.Password.Length > 128)
        {
            errors["password"] = "Password must not exceed 128 characters";
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Request validation failed", errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
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

    private static void ValidateLoginRequest(LoginRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = "Email is required";
        }
        else if (!IsValidEmail(request.Email))
        {
            errors["email"] = "Email is invalid";
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = "Password is required";
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(
                "Request validation failed",
                errors);
        }
    }
}