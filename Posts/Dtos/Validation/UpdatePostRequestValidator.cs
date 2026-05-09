using FluentValidation;

namespace UserBlog.Posts.Dtos;

public sealed class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Title is not null || x.Content is not null)
            .WithMessage("At least one field must be provided");

        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title must not be empty")
                .MaximumLength(120)
                .WithMessage("Title must not exceed 120 characters");
        });

        When(x => x.Content is not null, () =>
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content must not be empty")
                .MaximumLength(50_000)
                .WithMessage("Content must not exceed 50000 characters");
        });
    }
}