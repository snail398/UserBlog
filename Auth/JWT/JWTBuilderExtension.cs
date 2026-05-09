using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserBlog.Common;
using UserBlog.Common.Constants;

namespace UserBlog.Auth.JWT;

public static class JWTBuilderExtension
{
    public static void AddJWT(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
        
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

                        var errorCode = ErrorCodes.Unauthorized;
                        var message = "Access token is missing or invalid";

                        if (context.AuthenticateFailure is SecurityTokenExpiredException)
                        {
                            errorCode = ErrorCodes.AccessTokenExpired;
                            message = "Access token has expired";
                        }

                        await HttpErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, errorCode, message);
                    },

                    OnForbidden = async context =>
                    {
                        await HttpErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, "You do not have permission to access this resource");
                    }
                };
            });

        builder.Services.AddAuthorization();
    }
}
