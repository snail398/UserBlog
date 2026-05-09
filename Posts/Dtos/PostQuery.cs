using UserBlog.Data.Entities;

namespace UserBlog.Posts.Dtos;

public sealed class PostQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Search { get; init; }

    public PostStatus? Status { get; init; }
}