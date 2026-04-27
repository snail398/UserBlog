using Microsoft.AspNetCore.Mvc;
using UserBlog.Common.Exceptions;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "UserBlog.Api"
        });
    }
    
    [HttpGet("error")]
    public IActionResult Error()
    {
        throw new NotFoundException(
            "TEST_NOT_FOUND",
            "This is a test error");
    }
}