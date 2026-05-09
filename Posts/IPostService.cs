using UserBlog.Common;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

public interface IPostService
{
    Task<PostResponse> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<PostListItemResponse>> GetPublicPostsAsync(PostQuery query, CancellationToken cancellationToken = default);
}