using UserBlog.Data.Entities;

namespace UserBlog.Posts.Dtos;

public sealed class PostResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public PostStatus Status { get; init; }

    public PostAuthorResponse Author { get; init; } = null!;

    public DateTimeOffset? PublishedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}