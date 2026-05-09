namespace UserBlog.Posts.Dtos;

public sealed class UpdatePostRequest
{
    public string? Title { get; init; }

    public string? Content { get; init; }
}