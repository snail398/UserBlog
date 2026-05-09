using UserBlog.Data.Entities;

namespace UserBlog.Posts.Dtos;

public sealed class CreatePostRequest
{
    public string Title { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public PostStatus Status { get; init; } = PostStatus.Draft;
}