using UserBlog.Data.Entities;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

public static class PostMapper
{
    private const int ExcerptLength = 160;

    public static PostResponse ToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Status = post.Status,
            Author = ToAuthorResponse(post.Author),
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public static PostListItemResponse ToListItemResponse(Post post)
    {
        return new PostListItemResponse
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = CreateExcerpt(post.Content),
            Status = post.Status,
            Author = ToAuthorResponse(post.Author),
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    private static PostAuthorResponse ToAuthorResponse(User author)
    {
        return new PostAuthorResponse
        {
            Id = author.Id,
            Username = author.Username
        };
    }

    private static string CreateExcerpt(string content)
    {
        if (content.Length <= ExcerptLength)
        {
            return content;
        }

        return content[..ExcerptLength] + "...";
    }
}