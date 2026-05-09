using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using UserBlog.Common.Constants;

namespace UserBlog.Common;

public static class ValidationBuilderExtension
{
    public static void AddValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var details = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => ToCamelCase(x.Key),
                        x => x.Value!.Errors.First().ErrorMessage);

                var error = ApiError.Create(ErrorCodes.ValidationError, "Request validation failed", details);

                return new BadRequestObjectResult(error);
            };
        });
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}