using System.Text;
using System.Text.RegularExpressions;

namespace UserBlog.Common;

public static class SlugGenerator
{
    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "post";
        }

        var normalized = value.Trim().ToLowerInvariant();

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (char.IsWhiteSpace(character) || character == '-' || character == '_')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString();

        slug = Regex.Replace(slug, "-{2,}", "-"); 
        slug = slug.Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "post" : slug;
    }
}