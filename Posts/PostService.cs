using UserBlog.Common;
using UserBlog.Common.Exceptions;
using UserBlog.Data;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

public sealed class PostService : IPostService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public Task<PostResponse> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedResponse<PostListItemResponse>> GetPublicPostsAsync(PostQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}