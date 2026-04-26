using Microsoft.EntityFrameworkCore;

namespace UserBlog.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}