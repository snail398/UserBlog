namespace UserBlog.Common;

public interface ICurrentUser
{
    Guid UserId { get; }

    bool TryGetUserId(out Guid userId);
}