using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserBlog.Common;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

[ApiController]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PostResponse>>> Create(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await _postService.CreateAsync(request, cancellationToken);

        return Created($"/api/posts/{post.Id}", ApiResponse<PostResponse>.Success(post));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PostListItemResponse>>>> GetPublicPosts([FromQuery] PostQuery query, CancellationToken cancellationToken)
    {
        var posts = await _postService.GetPublicPostsAsync(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResponse<PostListItemResponse>>.Success(posts));
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<ApiResponse<PostResponse>>> GetById(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postService.GetByIdAsync(postId, cancellationToken);

        return Ok(ApiResponse<PostResponse>.Success(post));
    }

    [Authorize]
    [HttpPatch("{postId:guid}")]
    public async Task<ActionResult<ApiResponse<PostResponse>>> Update(Guid postId, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var post = await _postService.UpdateAsync(postId, request, cancellationToken);

        return Ok(ApiResponse<PostResponse>.Success(post));
    }

    [Authorize]
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> Delete(Guid postId, CancellationToken cancellationToken)
    {
        await _postService.DeleteAsync(postId, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPost("{postId:guid}/publish")]
    public async Task<ActionResult<ApiResponse<PostResponse>>> Publish(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postService.PublishAsync(postId, cancellationToken);

        return Ok(ApiResponse<PostResponse>.Success(post));
    }

    [Authorize]
    [HttpPost("{postId:guid}/unpublish")]
    public async Task<ActionResult<ApiResponse<PostResponse>>> Unpublish(Guid postId, CancellationToken cancellationToken)
    {
        var post = await _postService.UnpublishAsync(postId, cancellationToken);

        return Ok(ApiResponse<PostResponse>.Success(post));
    }
}