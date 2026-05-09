using Microsoft.EntityFrameworkCore;
using UserBlog.Common;
using UserBlog.Common.Constants;
using UserBlog.Common.Exceptions;
using UserBlog.Data;
using UserBlog.Data.Entities;
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

    public async Task<PostResponse> CreateAsync(CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser.UserId;
        var now = DateTimeOffset.UtcNow;

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = currentUserId,
            Title = request.Title.Trim(),
            Slug = SlugGenerator.Generate(request.Title),
            Content = request.Content.Trim(),
            Status = request.Status,
            PublishedAt = request.Status == PostStatus.Published ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Posts.Add(post);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdPost = await _dbContext.Posts
            .Include(x => x.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == post.Id, cancellationToken);

        if (createdPost is null)
        {
            throw new NotFoundException(ErrorCodes.PostNotFound, "Post not found");
        }

        return PostMapper.ToResponse(createdPost);
    }

    public async Task<PaginatedResponse<PostListItemResponse>> GetPublicPostsAsync(PostQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page;
        var pageSize = query.PageSize;

        var postsQuery = _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .Where(x => x.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            postsQuery = postsQuery.Where(x =>EF.Functions.ILike(x.Title, $"%{search}%") || EF.Functions.ILike(x.Content, $"%{search}%"));
        }

        var total = await postsQuery.CountAsync(cancellationToken);

        var posts = await postsQuery
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = posts.Select(PostMapper.ToListItemResponse).ToList();

        return new PaginatedResponse<PostListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PostResponse> GetByIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == postId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException(ErrorCodes.PostNotFound, "Post not found");
        }

        if (post.Status == PostStatus.Published)
        {
            return PostMapper.ToResponse(post);
        }

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            throw new NotFoundException(ErrorCodes.PostNotFound, "Post not found");
        }

        if (post.AuthorId != currentUserId)
        {
            throw new NotFoundException(ErrorCodes.PostNotFound, "Post not found");
        }

        return PostMapper.ToResponse(post);
    }

    public async Task<PaginatedResponse<PostListItemResponse>> GetMyPostsAsync(PostQuery query, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser.UserId;

        var page = query.Page;
        var pageSize = query.PageSize;

        var postsQuery = _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Author)
            .Where(x => x.AuthorId == currentUserId);

        if (query.Status is not null)
        {
            postsQuery = postsQuery.Where(x => x.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            postsQuery = postsQuery.Where(x =>EF.Functions.ILike(x.Title, $"%{search}%") || EF.Functions.ILike(x.Content, $"%{search}%"));
        }

        var total = await postsQuery.CountAsync(cancellationToken);

        var posts = await postsQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = posts
            .Select(PostMapper.ToListItemResponse)
            .ToList();

        return new PaginatedResponse<PostListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PostResponse> UpdateAsync(Guid postId, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await GetOwnedPostAsync(postId, cancellationToken);

        if (request.Title is not null)
        {
            var title = request.Title.Trim();

            post.Title = title;
            post.Slug = SlugGenerator.Generate(title);
        }

        if (request.Content is not null)
        {
            post.Content = request.Content.Trim();
        }

        post.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PostMapper.ToResponse(post);
    }

    public async Task DeleteAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetOwnedPostAsync(postId, cancellationToken);

        _dbContext.Posts.Remove(post);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PostResponse> PublishAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetOwnedPostAsync(postId, cancellationToken);

        ValidatePostCanBePublished(post);

        if (post.Status == PostStatus.Published)
        {
            return PostMapper.ToResponse(post);
        }

        var now = DateTimeOffset.UtcNow;

        post.Status = PostStatus.Published;
        post.PublishedAt = now;
        post.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PostMapper.ToResponse(post);
    }

    public async Task<PostResponse> UnpublishAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetOwnedPostAsync(postId, cancellationToken);

        if (post.Status == PostStatus.Draft)
        {
            return PostMapper.ToResponse(post);
        }

        post.Status = PostStatus.Draft;
        post.PublishedAt = null;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return PostMapper.ToResponse(post);
    }

    private async Task<Post> GetOwnedPostAsync(Guid postId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        var post = await _dbContext.Posts
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == postId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundException(ErrorCodes.PostNotFound, "Post not found");
        }

        if (post.AuthorId != currentUserId)
        {
            throw new ForbiddenException(ErrorCodes.Forbidden, "You can only modify your own posts");
        }

        return post;
    }

    private static void ValidatePostCanBePublished(Post post)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(post.Title))
        {
            errors["title"] = "Title is required before publishing";
        }

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            errors["content"] = "Content is required before publishing";
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException("Post cannot be published", errors);
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return _currentUser.TryGetUserId(out userId);
    }
}