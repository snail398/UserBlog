using UserBlog.Common;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

public interface IPostService
{
    Task<PostResponse> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<PostListItemResponse>> GetPublicPostsAsync(PostQuery query, CancellationToken cancellationToken = default);

    Task<PostResponse> GetByIdAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<PostListItemResponse>> GetMyPostsAsync(PostQuery query, CancellationToken cancellationToken = default);

    Task<PostResponse> UpdateAsync(Guid postId, UpdatePostRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<PostResponse> PublishAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<PostResponse> UnpublishAsync(Guid postId, CancellationToken cancellationToken = default);
}