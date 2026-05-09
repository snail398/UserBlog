using Microsoft.EntityFrameworkCore;
using UserBlog.Common;
using UserBlog.Common.Constants;
using UserBlog.Common.Exceptions;
using UserBlog.Data;
using UserBlog.Users.Dtos;

namespace UserBlog.Users;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UserService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAppException(ErrorCodes.Unauthorized, "Current user does not exist");
        }

        return UserMapper.ToResponse(user);
    }
}