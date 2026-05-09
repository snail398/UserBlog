using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserBlog.Common;
using UserBlog.Posts.Dtos;

namespace UserBlog.Posts;

[ApiController]
[Authorize]
[Route("api/me/posts")]
public sealed class MyPostsController : ControllerBase
{
    private readonly IPostService _postService;

    public MyPostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PostListItemResponse>>>> GetMyPosts([FromQuery] PostQuery query, CancellationToken cancellationToken)
    {
        var posts = await _postService.GetMyPostsAsync(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResponse<PostListItemResponse>>.Success(posts));
    }
}