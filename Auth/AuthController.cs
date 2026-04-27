using Microsoft.AspNetCore.Mvc;
using UserBlog.Auth.Dtos;
using UserBlog.Common;
using UserBlog.Users.Dtos;

namespace UserBlog.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(request, cancellationToken);

        return Created($"/api/users/{user.Id}", ApiResponse<UserResponse>.Success(user));
    }
}