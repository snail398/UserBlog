using FluentValidation;
using UserBlog.Data.Entities;

namespace UserBlog.Posts.Dtos;

public sealed class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(120)
            .WithMessage("Title must not exceed 120 characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(50_000)
            .WithMessage("Content must not exceed 50000 characters");

        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(PostStatus), status))
            .WithMessage("Status is invalid");
    }
}