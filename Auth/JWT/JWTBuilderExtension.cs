using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserBlog.Common;

namespace UserBlog.Auth.JWT;

public static class JWTBuilderExtension
{
    public static void AddJWT(this WebApplicationBuilder builder)
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        if (jwtOptions is null)
        {
            throw new InvalidOperationException("JWT options are not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.AccessTokenSecret))
        {
            throw new InvalidOperationException("JWT access token secret is not configured.");
        }

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.AccessTokenSecret)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var errorCode = "UNAUTHORIZED";
                        var message = "Access token is missing or invalid";

                        if (context.AuthenticateFailure is SecurityTokenExpiredException)
                        {
                            errorCode = "ACCESS_TOKEN_EXPIRED";
                            message = "Access token has expired";
                        }

                        await HttpErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, errorCode, message);
                    },

                    OnForbidden = async context =>
                    {
                        await HttpErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, "FORBIDDEN", "You do not have permission to access this resource");
                    }
                };
            });

        builder.Services.AddAuthorization();
    }
}
