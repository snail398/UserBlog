namespace UserBlog.Posts.Dtos;

public sealed class PostAuthorResponse
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;
}