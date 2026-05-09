using FluentValidation;

namespace UserBlog.Posts.Dtos;

public sealed class PostQueryValidator : AbstractValidator<PostQuery>
{
    public PostQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .WithMessage("Search must not exceed 200 characters");
    }
}