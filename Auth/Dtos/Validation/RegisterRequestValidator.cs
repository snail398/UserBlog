using FluentValidation;

namespace UserBlog.Auth.Dtos;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is invalid")
            .MaximumLength(320)
            .WithMessage("Email must not exceed 320 characters");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(32)
            .WithMessage("Username must not exceed 32 characters")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can contain only letters, numbers, underscore and dash");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .WithMessage("Password must not exceed 128 characters");
    }
}