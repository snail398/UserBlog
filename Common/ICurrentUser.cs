namespace UserBlog.Common;

public interface ICurrentUser
{
    Guid UserId { get; }
}