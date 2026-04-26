namespace UserBlog.Data.Entities;

public sealed class Post
{
    public Guid Id { get; set; }

    public Guid AuthorId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public PostStatus Status { get; set; } = PostStatus.Draft;

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public User Author { get; set; } = null!;
}