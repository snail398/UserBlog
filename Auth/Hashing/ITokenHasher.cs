namespace UserBlog.Auth;

public interface ITokenHasher
{
    string Hash(string token);
}